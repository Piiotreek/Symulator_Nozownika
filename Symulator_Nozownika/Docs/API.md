# Dokumentacja REST API

Ten dokument opisuje punkty końcowe (endpoints) REST API dostępne w projekcie.

## 1. Game Score API

Kontroler `GameScoreController` zarządza wynikami graczy, rankingami i statystykami.

### Zapisz wynik (zalogowany użytkownik)
**POST** `/GameScore/SaveScore`

Zapisuje wynik gry dla aktualnie zalogowanego użytkownika. Endpoint ten aktualizuje również statystyki gracza (całkowity wynik, liczba gier, streak), sprawdza i odblokowuje osiągnięcia oraz aktualizuje pozycję w rankingu.

**Body (JSON):**
```json
{
  "score": 1500,
  "playTimeSeconds": 125,
  "won": true,
  "clicks": 340
}
```

**Odpowiedź sukcesywna (JSON):**
```json
{
  "success": true,
  "newRecord": false, // true, jeśli to nowy najlepszy wynik gracza
  "achievements": [ /* lista odblokowanych osiągnięć */ ],
  "completedQuests": [ /* lista ukończonych questów */ ]
}
```

### Zapisz wynik (anonimowy użytkownik)
**POST** `/GameScore/SaveAnonymousScore`

Zapisuje wynik dla gracza, który nie jest zalogowany.

**Body (JSON):**
```json
{
  "score": 800,
  "playerName": "Anonimowy Wojownik"
}
```

### Pobierz ranking (API)
**GET** `/api/highscores`

Zwraca listę najlepszych wyników z możliwością sortowania i filtrowania.

**Parametry (query string):**
- `sortBy` (string, opcjonalny): Klucz sortowania. Dostępne wartości: `score` (domyślne), `playerName`, `country`, `date`.
- `direction` (string, opcjonalny): Kierunek sortowania. Dostępne wartości: `desc` (domyślne), `asc`.
- `limit` (int, opcjonalny): Liczba wyników do zwrócenia. Domyślnie `50`, maksymalnie `500`.

**Przykład:** `/api/highscores?sortBy=playerName&direction=asc&limit=20`

### Pobierz X najlepszych wyników
**GET** `/GameScore/GetTopScores`

Prosty endpoint zwracający określoną liczbę najlepszych wyników.

**Parametry (query string):**
- `limit` (int, opcjonalny): Liczba wyników do zwrócenia. Domyślnie `10`.

### Pobierz najlepszy wynik użytkownika
**GET** `/GameScore/GetUserHighScore`

Zwraca najwyższy wynik osiągnięty przez aktualnie zalogowanego użytkownika. Wymaga zalogowania.

### Przypnij wynik
**POST** `/GameScore/PinScore`

Zapisuje ("przypina") wybrany wynik na profilu użytkownika jako wart zapamiętania. Wymaga zalogowania.

**Body (JSON):**
```json
{
  "score": 2137,
  "note": "Mój rekordowy wynik z super combosami!"
}
```

### Odepnij wynik
**POST** `/GameScore/UnpinScore`

Usuwa wcześniej przypięty wynik z profilu użytkownika. Wymaga zalogowania.

**Body (JSON):**
```json
{
  "id": 123 // ID przypiętego wyniku
}
```

---

## 2. Reports API (System Zgłoszeń)

API dostępne pod adresem bazowym `/api/ReportsApi` pozwala na zarządzanie zgłoszeniami użytkowników i wiadomości oraz nakładanie i obsługę kar.

### Konwencje i bezpieczeństwo
- Wszystkie endpointy wymagają autoryzacji (zalogowania) `[Authorize]`.
- Endpointy administracyjne wymagają polityki `[Authorize(Policy = "AdminOnly")]`.
- Dane żądań (Request Body) muszą być w formacie JSON.

---

### Zgłaszanie (Dostępne dla każdego zalogowanego użytkownika)

#### Zgłoś wiadomość
**POST** `/api/ReportsApi/message`
Zgłasza wiadomość, która łamie regulamin.

**Body (JSON):**
```json
{
  "messageId": 123,
  "reason": "Powód zgłoszenia, np. Wulgaryzmy lub Spam."
}
```

#### Zgłoś użytkownika
**POST** `/api/ReportsApi/user`
Zgłasza innego użytkownika systemu.

**Body (JSON):**
```json
{
  "reportedUserId": 456,
  "reason": 1, // typ wyliczeniowy ReportReason
  "description": "Opcjonalny opis szczegółowy."
}
```

---

### Zarządzanie zgłoszeniami (Tylko Admin)

#### Wiadomości
- **GET** `/api/ReportsApi/messages/pending`: Pobiera oczekujące zgłoszenia wiadomości.
- **GET** `/api/ReportsApi/messages`: Pobiera wszystkie zgłoszenia wiadomości.
- **GET** `/api/ReportsApi/messages/{id}`: Pobiera szczegóły konkretnego zgłoszenia.
- **POST** `/api/ReportsApi/messages/{id}/approve`: Zatwierdza zgłoszenie.
- **POST** `/api/ReportsApi/messages/{id}/reject`: Odrzuca zgłoszenie.
- **POST** `/api/ReportsApi/messages/{id}/penalty`: Nakłada karę za zgłoszoną wiadomość.

#### Użytkownicy
- **GET** `/api/ReportsApi/users/pending`: Pobiera oczekujące zgłoszenia użytkowników.
- **GET** `/api/ReportsApi/users`: Pobiera wszystkie zgłoszenia użytkowników.
- **GET** `/api/ReportsApi/users/{id}`: Pobiera szczegóły zgłoszenia użytkownika.
- **POST** `/api/ReportsApi/users/{id}/approve`: Zatwierdza zgłoszenie użytkownika.
- **POST** `/api/ReportsApi/users/{id}/reject`: Odrzuca zgłoszenie użytkownika.

---

### Zarządzanie Karami (Tylko Admin)

- **POST** `/api/ReportsApi/penalties`: Ręcznie nakłada karę na użytkownika.
- **GET** `/api/ReportsApi/penalties`: Pobiera wszystkie nałożone kary.
- **GET** `/api/ReportsApi/penalties/{id}`: Pobiera szczegóły kary.
- **POST** `/api/ReportsApi/penalties/{id}/revoke`: Uchyla (odwołuje) nałożoną karę.
