// Получаем элемент доски
var boardElement = document.getElementById('go-board');
var boardSizeSelector = document.getElementById('sizeSelector');
var debugContainer = document.getElementById('debugout'); // Див контейнер для вывода логов
let moveHistory = [];
// Переменные для хранения количества захваченных камней
var blackCaptured = 0;
var whiteCaptured = 0;
var blackScoreElement = document.getElementById('blackScore'); // Элемент для отображения очков черных
var whiteScoreElement = document.getElementById('whiteScore'); // Элемент для отображения очков белых
var aiTactic = 'В начале игры за черных в Го основная тактика заключается в установлении контроля над углами и краями доски, так как эти позиции легче защитить и развить.';
var game;
var board;
// Функция для обновления отображения очков
function updateScores() {
    blackScoreElement.innerText = "Человек: " + blackCaptured;
    whiteScoreElement.innerText = "Машина: " + whiteCaptured;
}

// Функция для получения допустимого хода от ИИ с проверкой
async function getValidAiMove(playerMove, attempt = 0) {

    if (attempt >= 10) {
        alert("ИИ не нашел достойного хода, человек побеждает!");
        return null;
    }


  
    //  Transform the board state to match the expected format
    const boardState = board.getState().objects.map(row =>
        row.map(cell => cell.length > 0 ? cell[0].c : 0)
    );

    try {

        var body = JSON.stringify({
            Move: playerMove,
            Board: boardState,
            MoveHistory: moveHistory,
            AITactic: aiTactic,
            Attempt: attempt // Передаем индекс попытки для дополнительной логики на сервере
        });

        if (attempt == 0)
            moveHistory.push({ x: playerMove.x, y: playerMove.y, actor: 0, c: -1 });

        //debugContainer.innerText += "Исходящие данные: " + body + "\n";

        const response = await fetch('/Go/GetAiMove', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body
        });

        const jsonData = await response.text();

        //debugContainer.innerText += "Полученные от ИИ данные: " + jsonData + "\n";

        const data = JSON.parse(jsonData);

        // Проверка допустимости хода
        if (game.isValid(data.aiMove.x, data.aiMove.y, data.aiMove.c)) {
            aiTactic = data.aiMove.t;
            document.getElementById('aiTactic').innerText = aiTactic;
            return data.aiMove;
        } else {
            // Если ход недопустим, делаем новый запрос
            return getValidAiMove(playerMove, attempt + 1);
        }
    } catch (error) {
        debugContainer.innerText += "'Ошибка при получении хода от ИИ:" + error + "\n";
    }
}

function changeLoadingState(bool) {
    if (bool) {
        document.querySelector('.wgo-board').classList.add('loading');
    }
    else {
        document.querySelector('.wgo-board').classList.remove('loading');
    }
}

async function playerMove(x, y) {
    var moveResult = game.play(x, y, game.turn);

    if (moveResult && moveResult instanceof Array) {
        changeLoadingState(true);

        if (moveResult.length > 0) {
            moveResult.forEach(move => {
                board.removeObject({ x: move.x, y: move.y });
            });
            updateCapturedStones();
        }

        board.addObject({ x, y, c: game.turn });
        board.redraw();

        //debugContainer.innerText += "Ход игрока: " + x + ", " + y + "\n";
        try {
            const aiMove = await getValidAiMove({ x: x, y: y, a: 0, c: game.turn });
            if (aiMove) {
                moveResult = game.play(aiMove.x, aiMove.y);
                if (moveResult.length > 0) {
                    moveResult.forEach(move => {
                        board.removeObject({ x: move.x, y: move.y });
                    });
                    updateCapturedStones();
                }
                board.addObject({ x: aiMove.x, y: aiMove.y, c: game.turn });
                board.redraw();
                moveHistory.push({ x: aiMove.x, y: aiMove.y, actor: 1, c: aiMove.C });
                updateCapturedStones();
                changeLoadingState();
            }
        } catch (e) {
            //удалить последний ход
            board.removeObject({ x, y });
            board.redraw();
        }


    }
    else if (moveResult === 1) {
        alert("Координаты за пределами игровой доски!");
    }
    else if (moveResult === 2) {
        alert("Клетка занята, выберите другую");
    }
    else if (moveResult === 3) {
        alert("Правило Ко запрещает ход - суицид");
    }
    else if (moveResult === 4) {
        alert("Правило Ко запрещает ход - суперко");
    }
}

// Функция для обновления количества захваченных камней
function updateCapturedStones() {
    whiteCaptured = game.getCaptureCount(WGo.W); // Захваченные белые камни;
    blackCaptured = game.getCaptureCount(WGo.B); // Захваченные черные камни;
    updateScores(); // Обновляем отображение очков
}

function newGame() {
    boardSizeValue = boardSizeSelector.selectedOptions[0].value | 1;
    boardElement.innerHTML = '';
    game = new WGo.Game(boardSizeValue);
    switch (boardSizeValue) {
        case 9:
            board = new WGo.Board(boardElement, {
                width: 400,
                height: 400,
                size: boardSizeValue
            });
            break;
        case 13:
            board = new WGo.Board(boardElement, {
                width: 600,
                height: 600,
                size: boardSizeValue
            });
            break;
        case 19:
            board = new WGo.Board(boardElement, {
                width: 800,
                height: 800,
                size: boardSizeValue
            });
            break;
        default: break;
    }

    var coordinates = {
        grid: {
            draw: function (args, board) {
                var t, xright, xleft, ytop, ybottom;

                this.fillStyle = "rgba(0,0,0,0.7)";
                this.textBaseline = "middle";
                this.textAlign = "center";
                this.font = board.stoneRadius + "px " + (board.font || "");

                xright = board.getX(-0.4);
                xleft = board.getX(board.size - 0.55);
                ytop = board.getY(-0.4);
                ybottom = board.getY(board.size - 0.55);

                for (var i = 0; i < board.size; i++) {
                    // Вертикальные координаты (оси Y) нумеруются сверху вниз, начиная с 0
                    t = board.getY(i);
                    this.fillText(i, xright, t);  // Сверху вниз
                    this.fillText(i, xleft, t);   // Сверху вниз

                    // Горизонтальные координаты (оси X) остаются как есть — от 0 слева направо
                    t = board.getX(i);
                    this.fillText(i, t, ytop);    // Слева направо
                    this.fillText(i, t, ybottom); // Слева направо
                }

                this.fillStyle = "black";
            }
        }
    };
    //var coordinates = {
    //    grid: {
    //        draw: function (args, board) {
    //            var ch, t, xright, xleft, ytop, ybottom;

    //            this.fillStyle = "rgba(0,0,0,0.7)";
    //            this.textBaseline = "middle";
    //            this.textAlign = "center";
    //            this.font = board.stoneRadius + "px " + (board.font || "");

    //            xright = board.getX(-0.4);
    //            xleft = board.getX(board.size - 0.55);
    //            ytop = board.getY(-0.4);
    //            ybottom = board.getY(board.size - 0.55);

    //            for (var i = 0; i < board.size; i++) {
    //                ch = i + "A".charCodeAt(0);
    //                if (ch >= "I".charCodeAt(0)) ch++;

    //                t = board.getY(i);
    //                this.fillText(board.size - i, xright, t);
    //                this.fillText(board.size - i, xleft, t);

    //                t = board.getX(i);
    //                this.fillText(String.fromCharCode(ch), t, ytop);
    //                this.fillText(String.fromCharCode(ch), t, ybottom);
    //            }

    //            this.fillStyle = "black";
    //        }
    //    }
    //};

    board.addCustomObject(coordinates);

    board.addEventListener("click", function (x, y) {
        playerMove(x, y);
    });

    updateScores();
}

//функция загрузки состояния игры
