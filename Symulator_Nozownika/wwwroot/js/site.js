document.addEventListener('DOMContentLoaded', () => {
    const demoButtons = document.querySelectorAll('[data-start-demo]');
    const dropdownToggles = document.querySelectorAll('#accountMenuToggle');

    dropdownToggles.forEach(toggle => {
        const dropdownElement = toggle.closest('.dropdown');
        const menu = dropdownElement?.querySelector('.navbar-account-dropdown');
        let allowHide = false;

        if (!dropdownElement || !menu || typeof bootstrap === 'undefined') {
            return;
        }

        dropdownElement.addEventListener('show.bs.dropdown', () => {
            menu.classList.remove('closing');
        });

        dropdownElement.addEventListener('hide.bs.dropdown', event => {
            if (allowHide) {
                allowHide = false;
                menu.classList.remove('closing');
                return;
            }

            event.preventDefault();
            menu.classList.add('closing');

            window.setTimeout(() => {
                allowHide = true;
                bootstrap.Dropdown.getOrCreateInstance(toggle).hide();
            }, 220);
        });

        dropdownElement.addEventListener('hidden.bs.dropdown', () => {
            menu.classList.remove('closing');
        });
    });

    // Intercept clicks on links inside account dropdown and navigate after animation
    const accountDropdowns = document.querySelectorAll('.navbar-account-dropdown');
    accountDropdowns.forEach(menu => {
        menu.querySelectorAll('a[href]').forEach(link => {
            link.addEventListener('click', (e) => {
                const href = link.getAttribute('href');
                if (!href || href.startsWith('#') || href.startsWith('javascript:')) return;

                // For logout which is a form button, skip (it's not an <a>)
                e.preventDefault();

                // Give the dropdown animation time to play, then navigate
                setTimeout(() => {
                    window.location.href = href;
                }, 250);
            });
        });
    });

    demoButtons.forEach(button => {
        button.addEventListener('click', async () => {
            button.disabled = true;
            const originalText = button.textContent;
            button.textContent = 'Uruchamianie demo...';

            try {
                const response = await fetch('/Demo/StartDemo', {
                    method: 'POST',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                const result = await response.json();

                if (result.success) {
                    window.location.href = '/Account/SecurePage';
                    return;
                }

                alert(result.error || 'Nie udało się uruchomić konta demo.');
            } catch {
                alert('Wystąpił błąd podczas uruchamiania konta demo.');
            } finally {
                button.disabled = false;
                button.textContent = originalText;
            }
        });
    });
});
