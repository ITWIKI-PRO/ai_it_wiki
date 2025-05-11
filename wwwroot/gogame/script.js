// script.js
let board = [];
const size = 19;
const black = 'black';
const white = 'white';
let currentPlayer = black;

// Инициализация доски
function initBoard() {
    const goBoard = document.getElementById('go-board');
    goBoard.innerHTML = ''; // Очистка доски
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
}

// Ход игрока
// Основная логика хода
function makeMove(row, col) {
    if (board[row][col] === null) {
        // Проверяем на запрещённые ходы (ко и самоубийство)
        if (isSuicideMove(board, row, col, currentPlayer)) {
            alert("Ход невозможен: самоубийственный ход");
            return;
        }

        // Симулируем ход
        const newBoard = JSON.parse(JSON.stringify(board));
        newBoard[row][col] = currentPlayer;

        if (isKoMove(newBoard)) {
            alert("Ход невозможен: правило ко");
            return;
        }

        // Если ход допустим, ставим камень
        board[row][col] = currentPlayer;
        updatePreviousBoard(board);
        renderBoard();
        saveGameState();  // Сохраняем игру после каждого хода
        switchPlayer();  // Переключаем ход игрока

    } else if (board[row][col] === currentPlayer) {
        // Если в клетке есть камень текущего игрока, убираем его
        board[row][col] = null;
        renderBoard();
    } else {
        alert("Ход невозможен: не ваш камень");
    }
}


// Переключение между игроками
function switchPlayer() {
    currentPlayer = currentPlayer === black ? white : black;
}

// Отображение доски
function renderBoard() {
    const goBoard = document.getElementById('go-board');
    goBoard.childNodes.forEach(cell => {
        const row = cell.dataset.row;
        const col = cell.dataset.col;

        // Устанавливаем визуальные стили для каждого игрока
        if (board[row][col] === black) {
            cell.classList.add('stone-black');
            cell.classList.remove('stone-white');
        } else if (board[row][col] === white) {
            cell.classList.add('stone-white');
            cell.classList.remove('stone-black');
        } else {
            cell.classList.remove('stone-black', 'stone-white');
        }
    });
}

// Проверка на ко-правило
function isKoMove(board) {
    if (previousBoard && areBoardsEqual(previousBoard, board)) {
        return true;  // Ход запрещён по правилу ко
    }
    return false;
}

// Проверка на самоубийственный ход
function isSuicideMove(board, row, col, player) {
    const simulatedBoard = JSON.parse(JSON.stringify(board));
    simulatedBoard[row][col] = player;

    const hasLiberties = checkLiberties(simulatedBoard, row, col, player);
    return !hasLiberties;
}

// Переключение между игроками
function switchPlayer() {
    currentPlayer = currentPlayer === black ? white : black;
}

// Проверка свободных соседних клеток
function checkLiberties(board, row, col, player) {
    const visited = Array(board.length).fill().map(() => Array(board.length).fill(false));
    return dfsCheckLiberties(board, row, col, player, visited);
}

function dfsCheckLiberties(board, row, col, player, visited) {
    if (row < 0, row >= board.length, col < 0 || col >= board.length) {
        return false;
    }
    if (visited[row][col]) {
        return false;
    }
    visited[row][col] = true;

    if (board[row][col] === null) {
        return true;  // Нашли свободу
    }
    if (board[row][col] !== player) {
        return false;
    }
    return dfsCheckLiberties(board, row - 1, col, player, visited) ||
        dfsCheckLiberties(board, row + 1, col, player, visited) ||
        dfsCheckLiberties(board, row, col - 1, player, visited) ||
        dfsCheckLiberties(board, row, col + 1, player, visited);
}

// Сравнение досок для правила ко
function areBoardsEqual(board1, board2) {
    for (let i = 0; i < board1.length; i++) {
        for (let j = 0; j < board1[i].length; j++) {
            if (board1[i][j] !== board2[i][j]) {
                return false;
            }
        }
    }
    return true;
}

// Обновление предыдущего состояния доски после успешного хода
function updatePreviousBoard(board) {
    previousBoard = JSON.parse(JSON.stringify(board));  // Глубокое копирование доски
}


// Обработка хода ИИ
async function aiMove() {
    const move = await getAiMove();
    if (move) {
        const { row, col } = move;
        board[move.Row][move.Col] = currentPlayer;
        renderBoard();
        switchPlayer();
    }
}

// Взаимодействие с API OpenAI
async function getAiMove() {
    try {
        const response = await fetch('/Go/GetAiMove', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                board: board.map(row => row.map(cell => (cell === black ? 'X' : cell === white ? 'O' : '.')))
            })
        });
        return await response.json();
    } catch (error) {
        console.error('Ошибка при запросе к API OpenAI:', error);
    }
}


// Генерация подсказки для API на основе текущей доски
function generatePrompt() {
    let prompt = "Сыграем в ГО. Вот текущая доска (черные - X, белые - O):\n";
    board.forEach(row => {
        prompt += row.map(cell => (cell === black ? 'X' : cell === white ? 'O' : '.')).join(' ') + '\n';
    });
    prompt += "Твой ход (X):";
    return prompt;
}

// Парсинг ответа от OpenAI (в виде координат хода)
function parseMoveFromResponse(responseText) {
    const { row, col } = responseText;
    return { row, col };
}

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
        let c = ca[i];
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

function eraseCookie(name) {
    document.cookie = name + '=; Max-Age=-99999999;';
}

// Сохранение состояния доски в куки
function saveGameState() {
    const gameState = JSON.stringify(board);  // Преобразуем состояние доски в строку
    setCookie('goGameState', gameState, 7);  // Сохраняем на 7 дней
    setCookie('currentPlayer', currentPlayer, 7);  // Сохраняем текущего игрока
}

// Восстановление состояния игры из куки
function loadGameState() {
    const savedState = getCookie('goGameState');
    const savedPlayer = getCookie('currentPlayer');

    if (savedState && savedPlayer) {
        board = JSON.parse(savedState);  // Восстанавливаем доску
        currentPlayer = savedPlayer;     // Восстанавливаем текущего игрока
        renderBoard();                   // Отображаем восстановленную доску
    } else {
        initBoard();  // Если нет сохраненного состояния, инициализируем новую игру
    }
}

document.getElementById('ai-move').addEventListener('click', aiMove);
document.getElementById('new-game').addEventListener('click', () => {
    initBoard();
    eraseCookie('goGameState');
    eraseCookie('currentPlayer');
});

// Инициализация игры при загрузке страницы
window.onload = initBoard;
window.onload = loadGameState;  // Загружаем игру из куки, если она была сохранена
