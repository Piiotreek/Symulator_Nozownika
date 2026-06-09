# 🚩 System Zgłaszania Wiadomości i Użytkowników

## 📋 Przegląd

System umożliwia użytkownikom zgłaszanie treści naruszających zasady społeczności oraz profili użytkowników z problemami. Administratorzy mogą przeglądać zgłoszenia, zatwierdzać/odrzucać je oraz nakładać kary na violatorów.

## ✨ Główne funkcjonalności

### Dla użytkowników
- 🚩 **Zgłaszanie wiadomości** - zgłaszanie obraźliwych lub nieodpowiednich wiadomości z czatu
- 👤 **Zgłaszanie użytkowników** - zgłaszanie profilów z problemami
- 📝 **Dodawanie powodu** - opisanie szczegółów naruszenia

### Dla administratorów
- 📊 **Panel administracyjny** - przegląd wszystkich zgłoszeń
- ✅ **Zarządzanie zgłoszeniami** - zatwierdzanie, odrzucanie
- 🔨 **Nakładanie kar** - zawieszenia 1/7 dni, bany, bany z usunięciem
- ⏱️ **Automatyczne wygasanie** - background service sprawdza kary co godzinę

## 🔧 Instalacja

### Wymagania
- .NET 10
- SQL Server
- Entity Framework Core 10

### Kroki
1. Aplikacja automatycznie tworzy nowe tabele przy pierwszym uruchomieniu
2. Migracja: `20260604000000_AddReportingSystem`
3. Tabele: `MessageReports`, `UserReports`, `UserPenalties`

## 🎯 Przypadki użycia

### Scenariusz 1: Użytkownik zgłasza wiadomość
```
1. Użytkownik zalogowany
2. Otwiera czat klubu
3. Widzi wiadomość z obraźliwą treścią
4. Kliczy 🚩 przy wiadomości
5. Wpisuje powód zgłoszenia
6. Administratorzy zostają powiadomieni
```

### Scenariusz 2: Użytkownik zgłasza profil
```
1. Użytkownik zalogowany
2. Otwiera czat klubu
3. Widzi użytkownika z nieodpowiednim nickiem
4. Klika 👤 przy jego wiadomości
5. Wybiera kategorię naruszenia
6. Opisuje szczegóły
7. Admin bada i nakłada karę
```

### Scenariusz 3: Admin zarządza zgłoszeniami
```
1. Admin zalogowany
2. Klika na awatar -> "Panel administracyjny"
3. Przegląda oczekujące zgłoszenia
4. Zatwierdza lub odrzuca
5. Dla zatwierdzonych - nakłada karę
6. System automatycznie wygasa zawieszenia
```

## 📱 Interfejs użytkownika

### Czat klubu
```html
<!-- Przy każdej wiadomości -->
<div class="message-actions">
    <button onclick="reportMessage(...)">🚩 Zgłoś wiadomość</button>
    <button onclick="reportUser(...)">👤 Zgłoś użytkownika</button>
</div>
```

### Panel administracyjny
- **Tabela 1**: Oczekujące zgłoszenia wiadomości
- **Tabela 2**: Oczekujące zgłoszenia użytkowników
- **Tabela 3**: Aktywne kary

## 🔐 Bezpieczeństwo

| Operacja | Wymagana rola | Wymagana autentykacja |
|----------|-------------|----------------------|
| Zgłosić wiadomość | User+ | ✅ |
| Zgłosić użytkownika | User+ | ✅ |
| Zarządzać zgłoszeniami | Admin | ✅ |
| Nakładać kary | Admin | ✅ |

## 📊 Struktura danych

### MessageReport
```csharp
public int Id { get; set; }
public int ReportedMessageId { get; set; }
public int ReportedByUserId { get; set; }
public string Reason { get; set; } // 500 znaków
public DateTime CreatedAt { get; set; }
public ReportStatus Status { get; set; } // Pending, Approved, Rejected, Resolved
public int? ReviewedByAdminId { get; set; }
public DateTime? ReviewedAt { get; set; }
public string AdminNotes { get; set; } // 500 znaków
```

### UserReport
```csharp
public int Id { get; set; }
public int ReportedUserId { get; set; }
public int ReportedByUserId { get; set; }
public ReportReason Reason { get; set; } // Enum: 0-6
public string Description { get; set; } // 500 znaków
public DateTime CreatedAt { get; set; }
public ReportStatus Status { get; set; }
public int? ReviewedByAdminId { get; set; }
public DateTime? ReviewedAt { get; set; }
public string AdminNotes { get; set; }
```

### UserPenalty
```csharp
public int Id { get; set; }
public int UserId { get; set; }
public int AdminId { get; set; }
public PenaltyType Type { get; set; } // 0-3
public string Reason { get; set; }
public DateTime AppliedAt { get; set; }
public DateTime? ExpiresAt { get; set; } // null dla banów
public bool IsActive { get; set; }
public int? RelatedReportId { get; set; }
public string Notes { get; set; }
```

## 🔗 API Endpoints

### Dla użytkowników
```bash
POST /api/reportsapi/message
{
  "messageId": 123,
  "reason": "Zawiera obraźliwe słowa"
}

POST /api/reportsapi/user
{
  "reportedUserId": 456,
  "reason": 0,  // ReportReason enum
  "description": "Obraźliwy nick"
}
```

### Dla administratorów
```bash
GET    /api/reportsapi/messages/pending              # Oczekujące
GET    /api/reportsapi/messages                      # Wszystkie
GET    /api/reportsapi/messages/{id}                 # Szczegóły
POST   /api/reportsapi/messages/{id}/approve         # Zatwierdź
POST   /api/reportsapi/messages/{id}/reject          # Odrzuć

GET    /api/reportsapi/users/pending                 # Oczekujące
GET    /api/reportsapi/users                         # Wszystkie
GET    /api/reportsapi/users/{id}                    # Szczegóły
POST   /api/reportsapi/users/{id}/approve            # Zatwierdź
POST   /api/reportsapi/users/{id}/reject             # Odrzuć

POST   /api/reportsapi/penalties                     # Nałóż karę
GET    /api/reportsapi/penalties                     # Wszystkie
GET    /api/reportsapi/penalties/{id}                # Szczegóły
POST   /api/reportsapi/penalties/{id}/revoke         # Uchyl
```

## 🛠️ Konfiguracja

### W Program.cs
```csharp
// Rejestracja serwisów
builder.Services.AddScoped<IReportManagementService, ReportManagementService>();
builder.Services.AddScoped<IPenaltyExpirationService, PenaltyExpirationService>();
builder.Services.AddHostedService<PenaltyExpirationBackgroundService>();

// Polityka AdminOnly
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});
```

## 📋 Kategorie zgłoszeń użytkowników

```csharp
public enum ReportReason
{
    OffensiveNickname = 0,        // Obraźliwy nick
    InappropriateAvatar = 1,      // Nieodpowiedni awatar
    PornographicContent = 2,      // Pornografia na avarze
    Impersonation = 3,            // Podszywanie
    Harassment = 4,               // Nękanie
    Spam = 5,                     // Spam
    Other = 6                     // Inne
}
```

## ⏰ Typy kar

```csharp
public enum PenaltyType
{
    Suspension1Day = 0,        // 24 godziny zawieszenia
    Suspension7Days = 1,       // 7 dni zawieszenia
    Ban = 2,                   // Permanentny ban
    BanWithDeletion = 3        // Ban + usunięcie konta
}
```

## ⚙️ Background Service

**PenaltyExpirationBackgroundService** - sprawdza co godzinę:
- Czy zawieszenia wygasły
- Automatycznie deaktywuje wygasłe zawieszenia
- Loguje operacje

## 📊 Statystyki i monitorowanie

### Wskaźniki do śledzenia
- Liczba zgłoszeń na dzień
- Procent zatwierdzonych zgłoszeń
- Średni czas rozpatrzenia
- Liczba aktywnych kar
- Typowe kategorie zgłoszeń

## 🔄 Workflow zgłoszenia

```
Użytkownik zgłasza
       ↓
Status: Pending (oczekujące)
       ↓
Admin przegląda
       ↓
Admin zatwierdza lub odrzuca
       ↓
Jeśli zatwierdzone → Admin nakłada karę
       ↓
Status: Approved/Resolved
```

## 🚨 Troubleshooting

### Problem: Przycisk 🚩 nie pojawia się
- Sprawdź czy jesteś zalogowany
- Sprawdź czy masz dostęp do czatu (jesteś członkiem klubu)
- Odśwież stronę

### Problem: Zgłoszenie nie wysyła się
- Sprawdź konsolę przeglądarki (F12 → Console)
- Sprawdź czy aplikacja ma dostęp do API
- Upewnij się że pole powodu nie jest puste

### Problem: Panel administracyjny nie ładuje się
- Sprawdź czy masz rolę Admin
- Sprawdź czy jesteś zalogowany
- Przejrzyj logi aplikacji

## 📝 Notatki

- Bany są permanentne (brak daty wygaśnięcia)
- Zawieszenia mają datę wygaśnięcia i wygasają automatycznie
- Każde zgłoszenie tworzy wpis w historii
- Administratorzy mogą dodawać notatki do zgłoszeń
- System obsługuje maksymalnie 500 znaków powodu/notatek

## 🔄 Aktualizacje w przyszłości

Planowane ulepszenia:
- [ ] Email notifications dla adminów
- [ ] Automatyczne wiadomości do użytkownika o karze
- [ ] Obsługa "Ban z usunięciem"
- [ ] Raportowanie (analytics)
- [ ] Appeal system (możliwość odwołania się od kary)

---

**Ostatnia aktualizacja**: 04.06.2026
**Wersja**: 1.0
**Status**: Production Ready ✅
