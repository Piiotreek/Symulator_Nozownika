# Integracja High Score do Gry - Instrukcja

## Jak używać High Score System

### 1. Dodaj script do swojego widoku gry (Game.cshtml lub Index.cshtml)

```html
@section Scripts {
    <script src="~/js/highscore.js"></script>
}
```

### 2. Zapisz wynik na koniec gry

W twoim kodzie JavaScript, po zakończeniu gry, wywołaj:

```javascript
// Gdzie 'finalScore' to liczba punktów
submitGameScore(finalScore);
```

**Przykład:**
```javascript
function endGame(playerScore) {
    // ... Twoja logika końca gry
    submitGameScore(playerScore);  // Zapisz wynik
}
```

### 3. Wyświetl ranking graczy

Dodaj do HTML'a kontener:
```html
<div id="highScoresContainer"></div>
```

I załaduj ranking:
```javascript
displayHighScores('highScoresContainer');
```

### 4. Wyświetl najlepszy wynik zalogowanego gracza

```javascript
getUserHighScore().then(score => {
    console.log('Twój najlepszy wynik:', score);
    // Wyświetl wynik na stronie
});
```

### 5. Obsługa zalogowania z pending score

Dodaj to do strony, na którą przekieruj użytkownika po zalogowaniu:

```html
@section Scripts {
    <script src="~/js/highscore.js"></script>
    <script>
        // Sprawdź czy jest wynik do zapisu
        checkPendingScore();
    </script>
}
```

---

## Jak to działa?

1. **Zalogowany użytkownik:**
   - Wynik jest automatycznie zapisywany
   - System sprawdza czy nowy wynik jest lepszy niż poprzedni
   - Jeśli tak - zapisuje nowy top score
   - Jeśli nie - pokazuje komunikat z aktualnym najlepszym wynikiem

2. **Niezalogowany użytkownik:**
   - Pojawia się popup z pytaniem "Chcesz się zalogować?"
   - Jeśli kliknie OK - przeniesiony do logowania, wynik zostaje tymczasowo zapisany
   - Po zalogowaniu - wynik automatycznie się zapisuje
   - Jeśli kliknie Anuluj - może zalogować wynik anonimowo (opcjonalnie)

3. **Anonimowy gracz:**
   - Może wpisać swoją nazwę
   - Wynik zostaje zapisany bez przypisania do konta

---

## API Endpoints

```
POST /GameScore/SaveScore
- Zapisuje wynik zalogowanego użytkownika

POST /GameScore/SaveAnonymousScore  
- Zapisuje wynik anonimowego gracza

GET /GameScore/GetUserHighScore
- Pobiera najlepszy wynik zalogowanego użytkownika

GET /GameScore/GetTopScores?limit=10
- Pobiera top N wyników
```

---

## Przykład pełnej integracji w stronie gry:

```html
@{
    ViewData["Title"] = "Gra";
}

<div class="game-container">
    <div id="game"></div>
    <div id="scoreBoard">
        <p>Wynik: <span id="currentScore">0</span></p>
    </div>
</div>

<div id="gameOver" style="display:none;">
    <h2>Koniec gry!</h2>
    <p>Twój wynik: <span id="finalScore"></span></p>
    <button onclick="location.href='/'">Powrót do menu</button>
</div>

<div style="margin-top: 40px;">
    <h3>Ranking - Top 10</h3>
    <div id="highScoresContainer"></div>
</div>

@section Scripts {
    <script src="~/js/highscore.js"></script>
    <script>
        let score = 0;

        function gameLogic() {
            // Twoja logika gry
            score += 10; // Przykład
            document.getElementById('currentScore').textContent = score;
        }

        function finishGame() {
            document.getElementById('gameOver').style.display = 'block';
            document.getElementById('finalScore').textContent = score;
            
            // Zapisz wynik
            submitGameScore(score);
            
            // Odśwież ranking
            displayHighScores('highScoresContainer');
        }

        // Załaduj ranking na starcie
        displayHighScores('highScoresContainer');
        
        // Sprawdź pending score
        checkPendingScore();
    </script>
}
```
