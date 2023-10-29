// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function flipTilesSequentially(rowIndex, keyboard, correct) {
    var row = document.querySelectorAll('.wordle-grid .row')[rowIndex];
    var tiles = row.querySelectorAll('.tile');
    var delay = 100;  // initial delay
    toggleButtons(true, keyboard);

    tiles.forEach((tile,index) => {
        setTimeout(() => {
            tile.style.transform = 'rotateY(180deg)';
            
            // Add an event listener to handle the middle of the transition
            tile.addEventListener('transitionend', function handleFlip() {
                var tileFront = tile.querySelector('.tile-front');
                var tileBack = tile.querySelector('.tile-back');

                if (tile.style.transform === 'rotateY(180deg)') {
                    tileFront.style.zIndex = 1;
                    tileBack.style.zIndex = 2;
                } else {
                    tileFront.style.zIndex = 2;
                    tileBack.style.zIndex = 1;
                }

                // Remove the event listener after it's executed to prevent multiple bindings
                tile.removeEventListener('transitionend', handleFlip);

                if (index === tiles.length - 1) {
                    // Re-enable all buttons after the last tile has completed its transition
                    toggleButtons(false, keyboard);
                    if (correct)
                        jumpTilesSequentially(rowIndex)
                }
            });

        }, delay);

        delay += 350;  // delay for the next tile
    });
}

function toggleButtons(disable, keyboard) {
    // Get all buttons within the keyboard div
    var buttons = document.querySelectorAll('.keyboard button');

    buttons.forEach(button => {
        if (!disable) {
            if (button.classList.contains('enter-key') || button.classList.contains('delete-key')) {
                button.disabled = false;
            }
            else {
                button.disabled = !keyboard.find(key => key.value === button.innerText).active;
            }
        }
        else {
            button.disabled = disable;
        }
    });
}
function jumpTilesSequentially(rowIndex) {
    var row = document.querySelectorAll('.wordle-grid .row')[rowIndex];
    var tiles = row.querySelectorAll('.tile');
    var delay = 100;  // initial delay

    tiles.forEach((tile, index) => {
        setTimeout(() => {
            tile.style.animation = 'jump 0.5s';
            var tileFront = tile.querySelector('.tile-front');
            var tileBack = tile.querySelector('.tile-back');
            let color = getComputedStyle(document.documentElement).getPropertyValue('--green-color').trim();

            tileFront.style.backgroundColor = color;

            // Remove the animation once it completes to allow re-triggering
            tile.addEventListener('animationend', function handleAnimationEnd() {
                tile.style.animation = '';
                tile.removeEventListener('animationend', handleAnimationEnd);
                tileBack.style.backgroundColor = 'hotpink';
                if (index === tiles.length - 1) { // Check if it's the last tile
                    setTimeout(function () {
                        document.getElementById("overlayModal").style.display = "block";
                    }, 1000);
                }
            });

        }, delay);

        delay += 100;  // delay for the next tile
    });
}

function shakeRow(rowIndex) {
    var row = document.querySelectorAll('.wordle-grid .row')[rowIndex];
    var tiles = row.querySelectorAll('.tile');
    tiles.forEach(tile => {
        tile.classList.add("shake-tile");
        tile.addEventListener('animationend', function () {
            tile.classList.remove("shake-tile");
        });
    });
}

