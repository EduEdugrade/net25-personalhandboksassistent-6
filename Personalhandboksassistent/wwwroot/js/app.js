const userQuestion = document.querySelector(".userQuestion");
const submitButton = document.querySelector(".submitButton");
const chatBox = document.getElementById('chat-box');
const logoutButton = document.querySelector(".logout-button");
const addDocumentButton = document.querySelector(".add-document");
const documents = document.querySelector(".documents");
const clearHistoryButton = document.querySelector(".clearHistory");

const deleteModal = document.getElementById("deleteModal");
const cancelDelete = document.getElementById("cancelDelete");
const confirmDelete = document.getElementById("confirmDelete");
let documentToDelete = null;

const loginModal = document.getElementById("loginModal");
const loginForm = document.getElementById("loginForm");
const loginError = document.getElementById("loginError");

if (!getToken()) { showLoginModal(); } else { hideLoginModal(); }
const isAdmin = getRole() === "admin";

if (isAdmin) {
    document.body.classList.add("admin");
    loadDocuments();
}

submitButton.addEventListener("click", () => {
    getAiResponse(userQuestion.value);
});

logoutButton.addEventListener("click", () => {
    clearSession();
    document.body.classList.remove("admin");
    showLoginModal();
});

loginForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    const username = document.getElementById("username").value.trim();
    const password = document.getElementById("password").value;
    loginError.textContent = "";

    try {
        const response = await fetch("/login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                username,
                password
            })
        });

        if (!response.ok) {
            if (response.status === 401) {
                throw new Error("Fel användarnamn eller lösenord.");
            }

            throw new Error("Kunde inte logga in.");
        }

        const data = await response.json();

        setSession(data.token, data.user);
        hideLoginModal();

        if (data.user.role === "admin") {
            document.body.classList.add("admin");
            loadDocuments();
        }
    } catch (error) {
        loginError.textContent = error.message;
    }
});

function showLoginModal() {
    loginModal.classList.remove("hidden");
    document.body.style.overflow = "hidden";
}

function hideLoginModal() {
    loginModal.classList.add("hidden");
    document.body.style.overflow = "";
}

clearHistoryButton.addEventListener("click", async () => {
    try {
        const response = await authFetch("/api/history", {
            method: "DELETE"
        });

        if (!response.ok) {
            throw new Error("Kunde inte rensa chatthistoriken.");
        }

        const data = await response.json();

        chatBox.innerHTML = `
            <div class="welcome-message">
                <h2>Hej! Hur kan jag hjälpa dig?</h2>
                <p>
                    Jag kan svara på frågor utifrån dokumenten
                    i personalhandboken.
                </p>
            </div>
        `;
        console.log(data.message);
    } catch (error) {
        console.error("Fel vid rensning av historik:", error);
    }
});

addDocumentButton.addEventListener("click", () => {
    const fileInput = document.getElementById('fileInput');

    fileInput.addEventListener('change', async (event) => {
        const file = event.target.files[0];

        if (file) await addDocument(file);
    }, { once: true });

    fileInput.click();
});

async function addDocument(file) {
    try {
        const formData = new FormData();
        formData.append("file", file);

        const response = await authFetch("/api/addFile", {
            method: "POST",
            body: formData
        });

        if (!response.ok) throw new Error("Kunde inte ladda upp filen");

        const data = await response.json();
        console.log("Uppladdning lyckades!", data);

        await loadDocuments();
    } catch (error) {
        console.error("Fel vid tillägg av dokument:", error);
    }
}

async function deleteDocument(id) {
    try {
        const response = await authFetch(`/api/documents/${encodeURIComponent(id)}`, {
            method: "DELETE"
        });

        if (!response.ok) throw new Error(`Fel: ${response.status}`);

        const result = await response.json();

        console.log("Dokument raderat:", result);
        loadDocuments();
    } catch (error) {
        console.error("Kunde inte radera dokumentet: ", error);
    }
}

async function loadDocuments() {
    try {
        const response = await authFetch("/api/documents");

        if (!response.ok) {
            throw new Error("Kunde inte hämta dokumenten");
        }

        const documents = await response.json();
        renderDocuments(documents);
    } catch (error) {
        console.error("Kunde inte ladda dokumenten: ", error);
    }
}

async function getAiResponse(question) {
    try {
        const response = await authFetch("/api/answer", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                message: question
            })
        });

        const data = await response.json();
        sendMessage(question, data.answer);
    } catch (error) {
        console.error("Fetch-fel:", error);
    }
}

function sendMessage(question, aiAnswer) {
    const userMessage = document.createElement('div');
    userMessage.textContent = question;
    userMessage.classList.add('chat-message', 'sent');

    chatBox.appendChild(userMessage);
    chatBox.scrollTop = chatBox.scrollHeight;

    const chatbotResponse = document.createElement('div');
    chatbotResponse.textContent = aiAnswer;
    chatbotResponse.classList.add('chat-message', 'received');

    chatBox.appendChild(chatbotResponse);
    chatBox.scrollTop = chatBox.scrollHeight;

    userQuestion.value = '';
}

function renderDocuments(documentList) {
    documents.innerHTML = "";

    documentList.forEach(documentItem => {
        const documentElement = document.createElement("div");
        documentElement.classList.add("document");

        documentElement.innerHTML = `
            <div class="document-icon">📄</div>

            <div class="document-info">
                <span class="document-name"></span>
                <span class="document-meta"></span>
            </div>

            <button 
                class="delete-document" 
                type="button"
                title="Ta bort dokument">
                x
            </button>
        `;

        documentElement.querySelector(".document-name").textContent = documentItem.name;

        documentElement.querySelector(".document-meta").textContent = documentItem.type;

        documentElement.querySelector(".delete-document")
            .addEventListener("click", () => {
                documentToDelete = documentItem.name;
                deleteModal.classList.add("active");
            });

        documents.appendChild(documentElement);
    })
}

cancelDelete.addEventListener("click", () => {
    deleteModal.classList.remove("active");
    documentToDelete = null;
});

confirmDelete.addEventListener("click", () => {
    deleteModal.classList.remove("active");

    if (documentToDelete) {
        deleteDocument(documentToDelete);
        documentToDelete = null;
    }
});
