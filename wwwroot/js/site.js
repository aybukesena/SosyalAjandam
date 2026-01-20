$(document).ready(function () {
    // Aura Click Event
    const messages = [
        "Bugün harika bir gün, hadi başlayalım!",
        "Odaklan ve başar!",
        "Her adım seni hedefine yaklaştırır.",
        "Mola vermeyi unutma, zihin dinlenmeli.",
        "Senin potansiyelin sınırsız!",
        "Küçük ilerlemeler büyük sonuçlar doğurur.",
        "Bugün kendin için ne yaptın?",
        "Hata yapmak, öğrenmenin bir parçasıdır."
    ];

    // Build speech bubble dynamically
    const bubble = $(`
        <div id="aura-bubble" class="glass-panel text-white p-3 position-absolute" 
             style="bottom: 80px; right: 0; width: 250px; display: none; z-index: 1000; border-radius: 15px 15px 0 15px;">
            <p class="mb-0 small" id="aura-text"></p>
        </div>
    `);

    $('#aura-container').append(bubble);

    $('.aura-sphere').click(function () {
        const randomMsg = messages[Math.floor(Math.random() * messages.length)];
        $('#aura-text').text(randomMsg);
        $('#aura-bubble').fadeIn(300).delay(3000).fadeOut(300);

        // Bonus pulse on click
        $(this).css('transform', 'scale(0.9)');
        setTimeout(() => {
            $(this).css('transform', 'scale(1)');
        }, 200);
    });
});
