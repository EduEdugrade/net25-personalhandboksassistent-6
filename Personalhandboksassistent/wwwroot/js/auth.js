const TOKEN_KEY = "app_token";
const ROLE_KEY = "app_role";
const NAME_KEY = "app_user";

function getToken() {
   return localStorage.getItem(TOKEN_KEY);
}

function getRole() {
   return localStorage.getItem(ROLE_KEY);
}

function setSession(token, user) {
   localStorage.setItem(TOKEN_KEY, token);
   localStorage.setItem(ROLE_KEY, user.role);
   localStorage.setItem(NAME_KEY, user.username);
}

function clearSession() {
   localStorage.removeItem(TOKEN_KEY);
   localStorage.removeItem(ROLE_KEY);
   localStorage.removeItem(NAME_KEY);
}

async function authFetch(url, options = {}) {
   const response = await fetch(url, {
      ...options,
      headers: {
         ...(options.headers || {}),
         Authorization: `Bearer ${getToken()}`,
      },
   });

   if (response.status === 401) {
      clearSession();
      location.reload();
   }

   if (response.status === 403) {
      throw new Error("Du har inte behörighet.");
   }

   return response;
}
