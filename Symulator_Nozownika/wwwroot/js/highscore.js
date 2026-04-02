// Przechowywanie tymczasowego wyniku dla niezalogowanych użytkowników
let tempScore = 0;

// Funkcja wywoływana po zakończeniu gry
async function submitGameScore(score) {
    tempScore = score;
    console.log('📤 Wysyłam wynik:', score);

    try {
        // Pobierz token CSRF
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        const headers = {
            'Content-Type': 'application/json'
        };

        // Dodaj token CSRF jeśli istnieje
        if (token) {
            headers['RequestVerificationToken'] = token;
            console.log('✓ Token CSRF znaleziony');
        } else {
            console.log('⚠ Token CSRF nie znaleziony');
        }

        const payload = { score: score };
        console.log('📦 Payload:', JSON.stringify(payload));

        const response = await fetch('/GameScore/SaveScore', {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(payload)
        });

        console.log('📨 Status odpowiedzi:', response.status);

        const result = await response.json();
        console.log('📥 Odpowiedź serwera:', result);

        if (result.success) {
            showAlert('Sukces! ✓', result.message);
            return true;
        } else if (result.needsLogin) {
            // Pokaż popup pytając o zalogowanie
            showLoginPrompt(score);
        } else {
            showAlert('Wynik nie został zapisany', result.message);
        }
    } catch (error) {
        console.error('❌ Błąd przy wysyłaniu wyniku:', error);
        showAlert('Błąd', 'Nie udało się zapisać wyniku: ' + error.message);
    }
}

// Wyświetlanie popupu do logowania
function showLoginPrompt(score) {
    const userResponse = confirm(
        `Aby zapisać wynik (${score} pkt), musisz się zalogować.\n\n` +
        'Kliknij OK aby się zalogować, lub ANULUJ aby kontynuować bez zapisu.'
    );

    if (userResponse) {
        // Przechowaj wynik w sessionStorage przed przesunięciem
        sessionStorage.setItem('pendingScore', score);
        window.location.href = '/Account/Login';
    } else {
        // Gracz rezygnuje - może zaproponować anonimowy zapis
        showAnonymousScoreSave(score);
    }
}

// Opcjonalnie: zapis anonimowego wyniku
async function showAnonymousScoreSave(score) {
    const playerName = prompt('Wpisz swoją nazwę (opcjonalnie):');
    
    if (playerName !== null) {
        try {
            const response = await fetch('/GameScore/SaveAnonymousScore', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ 
                    score: score,
                    playerName: playerName || 'Anonimowy gracz'
                })
            });

            const result = await response.json();
            if (result.success) {
                showAlert('Sukces!', result.message);
            }
        } catch (error) {
            console.error('Błąd:', error);
        }
    }
}

// Pobierz najlepszy wynik zalogowanego użytkownika
async function getUserHighScore() {
    try {
        const response = await fetch('/GameScore/GetUserHighScore');
        const result = await response.json();
        
        if (result.success) {
            return result.highScore;
        }
    } catch (error) {
        console.error('Błąd przy pobieraniu najlepszego wyniku:', error);
    }
    return 0;
}

// Pobierz top 10 wyników
async function getTopScores() {
    try {
        const response = await fetch('/GameScore/GetTopScores?limit=10');
        const scores = await response.json();
        return scores;
    } catch (error) {
        console.error('Błąd przy pobieraniu rankingu:', error);
    }
    return [];
}

// Wyświetl ranking
async function displayHighScores(containerId) {
    const scores = await getTopScores();
    const container = document.getElementById(containerId);
    
    if (!container) return;

    if (scores.length === 0) {
        container.innerHTML = '<p>Brak wyników do wyświetlenia</p>';
        return;
    }

    let html = '<ol>';
    scores.forEach((score, index) => {
        html += `
            <li>
                <strong>${score.playerName}</strong> - ${score.score} pkt
                <small>${new Date(score.createdAt).toLocaleDateString('pl-PL')}</small>
            </li>
        `;
    });
    html += '</ol>';

    container.innerHTML = html;
}

// Funkcja pomocnicza do wyświetlania alertów
function showAlert(title, message) {
    alert(`${title}\n\n${message}`);
}

// Sprawdź czy jest pending score po zalogowaniu
async function checkPendingScore() {
    const pendingScore = sessionStorage.getItem('pendingScore');
    
    if (pendingScore) {
        sessionStorage.removeItem('pendingScore');
        
        try {
            const response = await fetch('/GameScore/SaveScore', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ score: parseInt(pendingScore) })
            });

            const result = await response.json();
            if (result.success) {
                showAlert('Sukces!', `Twój wynik (${pendingScore} pkt) został zapisany!`);
            }
        } catch (error) {
            console.error('Błąd przy zapisywaniu pending score:', error);
        }
    }
}
