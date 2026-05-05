// Przechowywanie tymczasowego wyniku dla niezalogowanych użytkowników
let tempScore = 0;

// Funkcja wywoływana po zakończeniu gry
async function submitGameScore(score,clicks) {
    tempScore = score;

    console.log(`📤 submitGameScore() wywoływana z wynikiem: ${score}`);

    try {
        // Pobierz CSRF token z meta tagu lub form
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || 
                      document.querySelector('meta[name="csrf-token"]')?.content;

        const headers = {
            'Content-Type': 'application/json'
        };

        if (token) {
            headers['X-CSRF-TOKEN'] = token;
            console.log(`🔐 CSRF token znaleziony: ${token.substring(0, 10)}...`);
        } else {
            console.log(`⚠️ CSRF token nie znaleziony`);
        }

        console.log(`📨 Wysyłam do /GameScore/SaveScore z wynikiem: ${score}`);

        const response = await fetch('/GameScore/SaveScore', {
            method: 'POST',
            headers: headers,
            body: JSON.stringify({ score: score,clicks:clicks })
        });

        console.log(`📥 Status odpowiedzi: ${response.status}`);
        const result = await response.json();
        console.log(`📥 Odpowiedź serwera:`, result);

        //show achievement
        if (result.achievements && result.achievements.length > 0) {
            result.achievements.forEach(ach => {
                let name = ach.name || ach.Name;
                let imagePath = ach.imagePath || ach.ImagePath;

                if (typeof showAchievementToast === "function") {
                    showAchievementToast(name, imagePath);
                } else {
                    console.error("Brak funkcji showAchievementToast!");
                }
            });
        }

        // slowed alert
        setTimeout(() => {
            if (result.success) {
                showAlert('Sukces! ✓', result.message);
            } else if (result.needsLogin) {
                //added clicks
                showLoginPrompt(score,clicks);
            } else {
                showAlert('Informacje o wyniku', result.message); 
            }
        }, 300);

    } catch (error) {
        console.error('❌ Błąd przy zapisywaniu wyniku:', error);
        showAlert('Błąd', 'Nie udało się zapisać wyniku');
    }
}

// Wyświetlanie popupu do logowania
function showLoginPrompt(score, clicks) {
    const userResponse = confirm(
        `Aby zapisać wynik (${score} pkt), musisz się zalogować.\n\n` +
        'Kliknij OK aby się zalogować, lub ANULUJ aby kontynuować bez zapisu.'
    );

    if (userResponse) {
        // Przechowaj wynik w sessionStorage przed przesunięciem
        sessionStorage.setItem('pendingScore', score, clicks);
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
                    //adding clicks to json
                    clicks: clicks,
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
    //added clicks check
    const pendingClicks = sessionStorage.getItem('pendingClicks') || 0;
    
    if (pendingScore) {
        sessionStorage.removeItem('pendingScore');
        sessionStorage.removeItem('pendingClicks');
        try {
            const response = await fetch('/GameScore/SaveScore', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ score: parseInt(pendingScore),clicks: parseInt(pendingClicks) })
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
