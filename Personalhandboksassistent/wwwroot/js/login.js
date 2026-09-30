var loginform = document.getElementById("loginForm");

loginform.addEventListener("submit", () => {
    login();
});

async function login() {
    const username = document.getElementById("username").value;
    const password = document.getElementById("password").value;

    const response = await fetch("/api/login", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            username,
            password
        })
    });

    if (response.ok) {
        window.location.href = "/";
    } else {
        document.getElementById("error").textContent ="Fel användarnamn eller lösenord";
    }
}