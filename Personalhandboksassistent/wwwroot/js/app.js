const userQuestion = document.querySelector(".userQuestion");
const submitButton = document.querySelector(".submitButton");
const chatBox = document.getElementById('chat-box');
const logoutButton = document.querySelector(".logout-button");
const deleteDocumentButton = document.querySelector(".delete-document");
const addDocumentButton = document.querySelector(".add-document");
const documents = document.querySelector(".documents");

const isAdmin = true; // "false" gömmer vänstra dokument panelen

if (isAdmin) {
    document.body.classList.add("admin");
}

submitButton.addEventListener("click", () => {
    getAiResponse(userQuestion.value);
    //sendMessage(userQuestion.value, "Hej jag är ai") //Ersätt till denna om du vill testa skicka meddelanden
});

logoutButton.addEventListener("click", () => {
    //Todo: Skicka tillbaka användaren till loginsidan
});

deleteDocumentButton.addEventListener("click", () => {
    //Todo: anropa deletedocument funktionen med ett id
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

        const response = await fetch("api/addFile", {
            method: "POST",
            body: formData
        });

        if (!response.ok) throw new Error("Kunde inte ladda upp filen");

        const data = await response.json();
        console.log("Uppladdning lyckades!", data);

    } catch (error) {
        console.error("Fel vid tillägg av dokument:", error);
    }
}

async function deleteDocument() {
    //Todo: fetch till api/delete
}

async function loadDocuments() {
    //Todo: Då sidan laddas in så ska man fetcha dokumenten från blobstorage? vene
}

async function getAiResponse(question) {
    try {
        const response = await fetch("/api/answer", {
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