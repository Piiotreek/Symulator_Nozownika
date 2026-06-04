# System Zgłaszania Wiadomości i Użytkowników

## Opis funkcjonalności

System umożliwia użytkownikom zgłaszanie:
- **Wiadomości z czatu klubu** - wiadomości zawierające treść obraźliwą lub naruszającą zasady
- **Użytkowników** - profili użytkowników z problemami takimi jak:
  - Obraźliwy nick
  - Nieodpowiedni awatar
  - Zawartość pornograficzna na avarze
  - Podszywanie się pod rzeczywistą osobę
  - Nękanie
  - Spam
  - Inne naruszenia

## Dla użytkowników

### Zgłaszanie wiadomości
1. W czacie klubu znajdź wiadomość którą chcesz zgłosić
2. Kliknij ikonę 🚩 (zgłoś wiadomość)
3. Wpisz powód zgłoszenia
4. Administratorzy przeanalizują Twoją skarżę

### Zgłaszanie użytkownika
1. W czacie klubu znajdź użytkownika którego chcesz zgłosić
2. Kliknij ikonę 👤 (zgłoś użytkownika)
3. Wybierz kategorię naruszenia
4. Opisz szczegóły problemu
5. Administratorzy przeanalizują Twoją skarżę

## Dla administratorów

### Dostęp do panelu
1. Zaloguj się na konto administratora
2. Kliknij na swój awatar w prawym górnym rogu
3. Kliknij "⚙️ Panel administracyjny"

### Zarządzanie zgłoszeniami

#### Zgłoszenia wiadomości
- Przeglądaj oczekujące zgłoszenia wiadomości
- Przejrzyj szczegóły: treść wiadomości, użytkownika, powód zgłoszenia
- **Zatwierdź** - zgłoszenie zostaje zatwierdzone (może prowadzić do usunięcia wiadomości)
- **Odrzuć** - zgłoszenie zostaje zamknięte bez działań

#### Zgłoszenia użytkowników
- Przeglądaj oczekujące zgłoszenia użytkowników
- Przejrzyj szczegóły: profil użytkownika, powód, opis
- **Zatwierdź** - zgłoszenie zatwierdzone, przygotuj karę
- **Odrzuć** - zgłoszenie zamknięte
- **Zastosuj karę** - przejdź do nakładania kary na użytkownika

### Nakładanie kar

Po zatwierdzeniu zgłoszenia możesz nałożyć karę:

**Typy kar:**
1. **Zawieszenie 1 dzień** - użytkownik nie może grać przez 24 godziny
2. **Zawieszenie 7 dni** - użytkownik nie może grać przez 7 dni
3. **Ban** - permanentny zakaz dostępu do konta
4. **Ban z usunięciem konta** - permanentny ban + usunięcie wszystkich danych użytkownika

**Proces:**
1. Wybierz typ kary
2. Wpisz powód
3. Dodaj notatki (opcjonalnie)
4. Kliknij "Zatwierdź"

### Zarządzanie karami
- Przeglądaj wszystkie aktywne i wygasłe kary
- Zobacz datę wygaśnięcia kary (zawieszenia)
- **Uchyl karę** - anuluj aktywną karę dla użytkownika

## Struktura bazy danych

### Tabele

**MessageReport** - Zgłoszenia wiadomości
- Id (int)
- ReportedMessageId (int) - ID wiadomości
- ReportedByUserId (int) - ID użytkownika zgłaszającego
- Reason (string) - Powód zgłoszenia
- CreatedAt (datetime)
- Status (enum: Pending, Approved, Rejected, Resolved)
- ReviewedByAdminId (int, nullable)
- ReviewedAt (datetime, nullable)
- AdminNotes (string, nullable)

**UserReport** - Zgłoszenia użytkowników
- Id (int)
- ReportedUserId (int) - ID zgłaszanego użytkownika
- ReportedByUserId (int) - ID użytkownika zgłaszającego
- Reason (enum: OffensiveNickname, InappropriateAvatar, PornographicContent, Impersonation, Harassment, Spam, Other)
- Description (string)
- CreatedAt (datetime)
- Status (enum: Pending, Approved, Rejected, Resolved)
- ReviewedByAdminId (int, nullable)
- ReviewedAt (datetime, nullable)
- AdminNotes (string, nullable)

**UserPenalty** - Kary dla użytkowników
- Id (int)
- UserId (int)
- AdminId (int) - ID administratora który nałożył karę
- Type (enum: Suspension1Day, Suspension7Days, Ban, BanWithDeletion)
- Reason (string)
- AppliedAt (datetime)
- ExpiresAt (datetime, nullable) - null dla banów
- IsActive (bool)
- RelatedReportId (int, nullable)
- Notes (string, nullable)

## API Endpoints

### Dla użytkowników

**POST /api/reportsapi/message**
```json
{
  "messageId": 123,
  "reason": "Zawiera obraźliwe słowa"
}
```

**POST /api/reportsapi/user**
```json
{
  "reportedUserId": 456,
  "reason": 0, // 0-6 enum ReportReason
  "description": "Użytkownik ma obraźliwy nick"
}
```

### Dla administratorów

**GET /api/reportsapi/messages/pending** - Oczekujące zgłoszenia wiadomości

**GET /api/reportsapi/messages** - Wszystkie zgłoszenia wiadomości

**GET /api/reportsapi/messages/{id}** - Szczegóły zgłoszenia

**POST /api/reportsapi/messages/{id}/approve** - Zatwierdź zgłoszenie
```json
{
  "notes": "Wiadomość usunięta"
}
```

**POST /api/reportsapi/messages/{id}/reject** - Odrzuć zgłoszenie
```json
{
  "notes": "Powód odrzucenia"
}
```

**GET /api/reportsapi/users/pending** - Oczekujące zgłoszenia użytkowników

**GET /api/reportsapi/users** - Wszystkie zgłoszenia użytkowników

**GET /api/reportsapi/users/{id}** - Szczegóły zgłoszenia

**POST /api/reportsapi/users/{id}/approve** - Zatwierdź zgłoszenie
```json
{
  "notes": "Zatwierdzony"
}
```

**POST /api/reportsapi/users/{id}/reject** - Odrzuć zgłoszenie
```json
{
  "notes": "Brak dowodów"
}
```

**POST /api/reportsapi/penalties** - Nałóż karę
```json
{
  "userId": 456,
  "type": 0, // 0-3 enum PenaltyType
  "reason": "Zgłoszenie nr 123",
  "relatedReportId": 123
}
```

**GET /api/reportsapi/penalties** - Wszystkie kary

**GET /api/reportsapi/penalties/{id}** - Szczegóły kary

**POST /api/reportsapi/penalties/{id}/revoke** - Uchyl karę

## Role i uprawnienia

- **User** - Może zgłaszać wiadomości i użytkowników
- **Admin** - Pełny dostęp do panelu administracyjnego, zarządzanie zgłoszeniami i karami

## Migracja bazy danych

Nowe tabele są tworzone automatycznie przy uruchomieniu aplikacji dzięki migracji:
```
20260604000000_AddReportingSystem
```

## Bezpieczeństwo

- Wszystkie zgłoszenia wymagają autentykacji
- Interfejs administracyjny jest chroniony polityką `AdminOnly`
- Dane są walidowane po stronie serwera
- API endpoints mają odpowiednie sprawdzenia autoryzacji
