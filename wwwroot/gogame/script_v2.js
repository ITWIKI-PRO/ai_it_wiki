var size = 9;
const black = 'X';
const white = 'O';
let board = [];
let currentPlayer = black;
let previousBoard = null;
let moveMade = false; // Проверка на количество ходов за один раз
let lastMoveRow = null;
let lastMoveCol = null;
let blackCapturedStones = 0;
let whiteCapturedStones = 0;
const komi = 6.5; // Коми для белого игрока
let boardHistory = [];

// Инициализация доски
function initBoard() {
    const goBoard = document.getElementById('go-board');
    goBoard.innerHTML = ''; // Очистка доски

    // Обновляем размер доски на основе выбора пользователя
    size = parseInt(document.getElementById('board-size').value, 20);

    // Обновляем размеры сетки для доски
    goBoard.style.gridTemplateColumns = `repeat(${size}, 30px)`;
    goBoard.style.gridTemplateRows = `repeat(${size}, 30px)`;

    board = Array(size).fill().map(() => Array(size).fill(null));

    for (let i = 0; i < size; i++) {
        for (let j = 0; j < size; j++) {
            const cell = document.createElement('div');
            cell.classList.add('cell');
            cell.dataset.row = i;
            cell.dataset.col = j;
            cell.addEventListener('click', () => makeMove(i, j));
            goBoard.appendChild(cell);
        }
    }
    clearDebugInfo();
    renderBoard();
    saveGameState();
}

function captureOpponentStones(row, col, currentPlayer) {
    const opponent = currentPlayer === black ? white : black;

    // Соседние группы камней соперника
    const directions = [
        [row - 1, col],
        [row + 1, col],
        [row, col - 1],
        [row, col + 1]
    ];

    directions.forEach(([r, c]) => {
        if (r >= 0 && r < size && c >= 0 && c < size && board[r][c] === opponent) {
            if (!checkLiberties(board, r, c, opponent)) {
                // Если группа не имеет свобод, удаляем её
                removeCapturedStones(r, c, opponent);
            }
        }
    });
}

function removeCapturedStones(row, col, player) {
    const visited = Array(board.length).fill().map(() => Array(board.length).fill(false));
    let capturedCount = 0;

    function dfsRemove(r, c) {
        if (r < 0 || r >= board.length || c < 0 || c >= board.length || visited[r][c] || board[r][c] !== player) {
            return;
        }
        visited[r][c] = true;
        board[r][c] = null; // Удаляем камень
        capturedCount++; // Увеличиваем счетчик захваченных камней

        // Рекурсивно удаляем всю группу
        dfsRemove(r - 1, c);
        dfsRemove(r + 1, c);
        dfsRemove(r, c - 1);
        dfsRemove(r, c + 1);
    }

    dfsRemove(row, col);

    // Учитываем захваченные камни
    if (player === black) {
        whiteCapturedStones += capturedCount;
    } else {
        blackCapturedStones += capturedCount;
    }
}

function calculateTerritory() {
    const visited = Array(size).fill().map(() => Array(size).fill(false));
    let blackTerritory = 0;
    let whiteTerritory = 0;

    function dfsTerritory(r, c, player) {
        if (r < 0 || r >= size || c < 0 || c >= size || visited[r][c]) {
            return true;
        }
        visited[r][c] = true;

        if (board[r][c] === black) return player === black;
        if (board[r][c] === white) return player === white;

        let result = true;
        result = dfsTerritory(r - 1, c, player) && result;
        result = dfsTerritory(r + 1, c, player) && result;
        result = dfsTerritory(r, c - 1, player) && result;
        result = dfsTerritory(r, c + 1, player) && result;

        if (result) {
            if (player === black) {
                blackTerritory++;
            } else {
                whiteTerritory++;
            }
        }
        return result;
    }

    // Проходим по всей доске и подсчитываем территорию
    for (let i = 0; i < size; i++) {
        for (let j = 0; j < size; j++) {
            if (board[i][j] === null && !visited[i][j]) {
                const surroundingPlayer = checkSurroundingStones(i, j);
                if (surroundingPlayer) {
                    dfsTerritory(i, j, surroundingPlayer);
                }
            }
        }
    }

    return { blackTerritory, whiteTerritory };
}

function checkSurroundingStones(row, col) {
    const directions = [
        [row - 1, col],
        [row + 1, col],
        [row, col - 1],
        [row, col + 1]
    ];
    let foundPlayer = null;

    directions.forEach(([r, c]) => {
        if (r >= 0 && r < size && c >= 0 && c < size && board[r][c] !== null) {
            if (foundPlayer === null) {
                foundPlayer = board[r][c];
            } else if (foundPlayer !== board[r][c]) {
                foundPlayer = null; // Если встречены камни обоих игроков, территория не засчитывается
            }
        }
    });

    return foundPlayer;
}

function calculateScore() {
    const { blackTerritory, whiteTerritory } = calculateTerritory();

    const blackScore = blackTerritory + blackCapturedStones;
    const whiteScore = whiteTerritory + whiteCapturedStones + komi; // Коми добавляется к очкам белого

    document.getElementById('black-score').textContent = `Черные: ${blackScore} (Территория: ${blackTerritory}, Захвачено: ${blackCapturedStones})`;
    document.getElementById('white-score').textContent = `Белые: ${whiteScore} (Территория: ${whiteTerritory}, Захвачено: ${whiteCapturedStones}, Коми: ${komi})`;
}

// Логика хода игрока (с обновленными проверками для отмены хода и ко)
function makeMove(row, col) {
    if (board[row][col] === null || board[row][col] === '.') {
        if (moveMade) {
            alert("Вы не можете сделать более одного хода за раз.");
            return;
        }

        if (isSuicideMove(board, row, col, currentPlayer)) {
            alert("Ход невозможен: самоубийственный ход");
            return;
        }

        const newBoard = JSON.parse(JSON.stringify(board));
        newBoard[row][col] = currentPlayer;

        if (!(row === lastMoveRow && col === lastMoveCol) && isKoMove(newBoard)) {
            alert("Ход невозможен: правило ко");
            return;
        }

        board[row][col] = currentPlayer;
        moveMade = true;
        lastMoveRow = row;
        lastMoveCol = col;
        updatePreviousBoard(board);

        // Проверка захвата соперника
        captureOpponentStones(row, col, currentPlayer);

        // Проверка захвата собственных камней после самоубийственного хода
        //captureOpponentStones(row, col, currentPlayer === black ? white : black);

        renderBoard();
        boardHistory.push(board.map(row => row.slice()));
        saveGameState();
    } else if (row === lastMoveRow && col === lastMoveCol) {
        board[row][col] = null;
        moveMade = false;
        renderBoard();
    } else {
        alert("Ход невозможен: не ваш камень");
    }
}
//function makeMove(row, col) {
//    if (
//        board[row][col] === "." || board[row][col] === null) {
//        if (moveMade) {
//            alert("Вы не можете сделать более одного хода за раз.");
//            return;
//        }
//        board[row][col] = currentPlayer;
//        moveMade = true;
//        lastMoveRow = row;
//        lastMoveCol = col;
//        updatePreviousBoard(board);
//        captureOpponentStones(row, col, currentPlayer);
//        renderBoard();
//        saveGameState();

//    } else {
//        alert("Ход невозможен.");
//    }
//}
// Отображение доски
function renderBoard() {
    const goBoard = document.getElementById('go-board');
    goBoard.childNodes.forEach(cell => {
        const row = cell.dataset.row;
        const col = cell.dataset.col;
        cell.className = 'cell';
        if (board[row][col] === black) {
            cell.classList.add('stone-black');
        } else if (board[row][col] === white) {
            cell.classList.add('stone-white');
        }
    });
}

// Переключение игроков
function switchPlayer() {
    currentPlayer = currentPlayer === black ? white : black;
}

// Проверка на правило ко
function isKoMove(board) {
    return previousBoard && areBoardsEqual(previousBoard, board);
}

// Проверка на самоубийственный ход
function isSuicideMove(board, row, col, player) {
    const simulatedBoard = JSON.parse(JSON.stringify(board));
    simulatedBoard[row][col] = player;
    return !checkLiberties(simulatedBoard, row, col, player);
}

// Проверка свободных клеток
function checkLiberties(board, row, col, player) {
    const visited = Array(board.length).fill().map(() => Array(board.length).fill(false));
    return dfsCheckLiberties(board, row, col, player, visited);
}

function dfsCheckLiberties(board, row, col, player, visited) {
    if (row < 0 || row >= board.length || col < 0 || col >= board.length || visited[row][col]) {
        return false;
    }
    visited[row][col] = true;
    if (board[row][col] === null) return true;
    if (board[row][col] !== player) return false;

    return dfsCheckLiberties(board, row - 1, col, player, visited) ||
        dfsCheckLiberties(board, row + 1, col, player, visited) ||
        dfsCheckLiberties(board, row, col - 1, player, visited) ||
        dfsCheckLiberties(board, row, col + 1, player, visited);
}

// Сравнение досок для правила ко
function areBoardsEqual(board1, board2) {
    for (let i = 0; i < board1.length; i++) {
        for (let j = 0; j < board1[i].length; j++) {
            if (board1[i][j] !== board2[i][j]) return false;
        }
    }
    return true;
}

function updatePreviousBoard(board) {
    //for (let i = 0; i < board.length; i++) {
    //    for (let j = 0; j < board[i].length; j++) {
    //        //if (board[i][j] === null) {
    //        //    board[i][j] = '.';
    //        //}
    //    }
    //}
    previousBoard = JSON.parse(JSON.stringify(board));
}

// Сохранение и загрузка состояния игры
function setCookie(name, value, days) {
    let expires = "";
    if (days) {
        const date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}

function getCookie(name) {
    const nameEQ = name + "=";
    const ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i].trim();
        if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length);
    }
    return null;
}

function saveGameState() {
    setCookie('goGameState', JSON.stringify(board), 7);
    setCookie('currentPlayer', currentPlayer, 7);
}

function loadGameState() {
    const savedState = getCookie('goGameState');
    const savedPlayer = getCookie('currentPlayer');

    if (savedState && savedPlayer) {
        board = JSON.parse(savedState);
        currentPlayer = savedPlayer;
        renderBoard();
    } else {
        initBoard();
    }
}

function appendDebugInfo(text) {
    let debugOut = document.getElementById('debug');
    let newLine = document.createElement('div');
    newLine.style.borderBottom = '1px solid #000';
    newLine.style.width = '300px';
    newLine.style.paddingBottom = '10px';
    newLine.style.paddingTop = '10px';
    newLine.textContent = text;
    debugOut.appendChild(newLine);
}

function clearDebugInfo() {
    let debugOut = document.getElementById('debug');
    debugOut.innerHTML = '';
}

// Ход ИИ
async function aiMove() {
    document.getElementById('go-board-wrapper').classList.add('loading')
    moveMade = false;  // Сбрасываем флаг после смены игрока
    switchPlayer();
    const move = await getAiMove();
    //добавляем состояние доски после хода ИИ в историю
    appendDebugInfo(JSON.stringify(move));
    if (move) {
        try {
            board[move.Row][move.Col] = currentPlayer;
        } catch (e) {
            try {
                board[move.row][move.col] = currentPlayer;
            } catch (e) {
                alert("ИИ ответил что-то невнятное...");
            }
        }
        boardHistory.push(board.map(row => row.slice()));
        calculateScore(); // Обновляем очки после каждого хода
        renderBoard();
        switchPlayer();
    }
    document.getElementById('go-board-wrapper').classList.remove('loading');
}

async function getAiMove() {
    try {
        let data = JSON.stringify({
            FirstIndex: '0,0',
            LastIndex: `${size - 1},${size - 1}`,
            board: board.map(row => row.map(cell => cell)),
            boardHistory: boardHistory // Передача истории на сервер
        });
        appendDebugInfo(data);
        const response = await fetch('/Go/GetAiMove', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: data
        });
        return await response.json();
    } catch (error) {
        alert(`Ошибка при запросе к API OpenAI: ${error}`);
    }
}

document.getElementById('ai-move').addEventListener('click', aiMove);
document.getElementById('new-game').addEventListener('click', () => {
    initBoard();
    setCookie('goGameState', '', -1);
    setCookie('currentPlayer', '', -1);
});

// Инициализация игры при загрузке
window.onload = loadGameState;
document.getElementById('new-game').click();