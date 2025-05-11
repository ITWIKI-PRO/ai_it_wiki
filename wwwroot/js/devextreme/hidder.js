$(document).ready(function () {
    // Добавление блока hidder при загрузке DOM
    $('body').prepend('<div id="hidder"></div>');

    // Установка начальных стилей для блока hidder
    $('#hidder').css({
        'position': 'absolute',
        'background': 'inherit',
        'width': '100%',
        'z-index': 10000
    });

    // Функция для обновления высоты блока hidder в зависимости от высоты элемента dx-license
    function updateHidderHeight() {
        var licenseHeight = $('dx-license').height();
        if (licenseHeight) {
            $('#hidder').css('height', (licenseHeight + 20) + 'px');
        }
    }

    // Подписка на событие изменения размера окна
    $(window).resize(function () {
        updateHidderHeight();
    });

    // Первоначальный вызов функции для установки правильной высоты при загрузке страницы
    updateHidderHeight();
});
