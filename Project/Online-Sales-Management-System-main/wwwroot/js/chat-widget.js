/* JS cho Chat Widget Pro */

document.addEventListener('DOMContentLoaded', function () {
    const chatToggleBtn = document.getElementById('chatToggleBtn');
    const chatBox = document.getElementById('chatBox');
    const closeChatMob = document.getElementById('closeChatMob');
    const chatInput = document.getElementById('chatInput');
    const sendBtn = document.getElementById('sendBtn');
    const chatBody = document.getElementById('chatBody');

    // --- HÀM XỬ LÝ CHÍNH ---

    // 1. Bật/Tắt hộp chat (Dùng class để kích hoạt CSS transition)
    function toggleChat() {
        chatBox.classList.toggle('active');
        chatToggleBtn.classList.toggle('opened');

        // Đổi icon nút toggle
        const icon = chatToggleBtn.querySelector('i');
        if (chatToggleBtn.classList.contains('opened')) {
            icon.classList.remove('lni-comments-alt');
            icon.classList.add('lni-close');
            // Focus vào ô nhập khi mở
            setTimeout(() => chatInput.focus(), 300);
        } else {
            icon.classList.add('lni-comments-alt');
            icon.classList.remove('lni-close');
        }
    }

    // 2. Gửi tin nhắn
    function sendMessage() {
        const message = chatInput.value.trim();
        if (message === "") return;

        // Thêm tin nhắn User
        appendMessage(message, 'user');
        chatInput.value = '';

        // Giả lập Support trả lời (Demo)
        showTypingIndicator();
        setTimeout(() => {
            hideTypingIndicator();
            appendMessage('Cảm ơn bạn. Chúng tôi đã nhận được tin nhắn và sẽ phản hồi sớm nhất!', 'support');
        }, 1500);
    }

    // Hỗ trợ: Thêm tin nhắn vào khung
    function appendMessage(text, sender) {
        const msgDiv = document.createElement('div');
        msgDiv.classList.add('message', sender);
        msgDiv.innerHTML = `
            ${text}
            <span class="message-time">${getCurrentTime()}</span>
        `;
        chatBody.appendChild(msgDiv);
        scrollToBottom();
    }

    // Hỗ trợ: Cuộn xuống cuối
    function scrollToBottom() {
        chatBody.scrollTop = chatBody.scrollHeight;
    }

    // Hỗ trợ: Lấy giờ
    function getCurrentTime() {
        const now = new Date();
        return now.getHours().toString().padStart(2, '0') + ":" +
            now.getMinutes().toString().padStart(2, '0');
    }

    // (Optional) Hiệu ứng "Đang nhập..."
    let typingDiv;
    function showTypingIndicator() {
        typingDiv = document.createElement('div');
        typingDiv.classList.add('message', 'support', 'typing');
        typingDiv.innerHTML = '<i class="lni lni-more-alt" style="font-size: 18px; opacity: 0.6;"></i>';
        chatBody.appendChild(typingDiv);
        scrollToBottom();
    }
    function hideTypingIndicator() {
        if (typingDiv) typingDiv.remove();
    }


    // --- GÁN SỰ KIỆN (EVENT LISTENERS) ---

    // Click nút tròn
    chatToggleBtn.addEventListener('click', toggleChat);

    // Click nút đóng trên mobile
    if (closeChatMob) {
        closeChatMob.addEventListener('click', toggleChat);
    }

    // Click nút gửi
    sendBtn.addEventListener('click', sendMessage);

    // Nhấn Enter để gửi
    chatInput.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') sendMessage();
    });
});