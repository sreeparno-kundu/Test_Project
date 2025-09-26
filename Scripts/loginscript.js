function validateForm() {
    let isValid = true;
    let userId = document.getElementById("UserId").value.trim();
    let password = document.getElementById("Password").value.trim();

    document.getElementById("userError").innerText = "";
    document.getElementById("passError").innerText = "";

    if (!userId) {
        document.getElementById("userError").innerText = "User ID is required";
        isValid = false;
    }
    if (!password) {
        document.getElementById("passError").innerText = "Password is required";
        isValid = false;
    }
    return isValid;
}



document.getElementById("loginForm").addEventListener("submit", async function (e) {
    e.preventDefault();

    if (!validateForm()) return;

    let data = {
        UserId: document.getElementById("UserId").value,
        Password: document.getElementById("Password").value
    };

    try {
        let response = await fetch('/Home/ApiLogin', {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        });

        let result = await response.json();

        let msgBox = document.getElementById("loginMessage");

        if (result.success) {
            msgBox.style.color = "lime";
            msgBox.innerText = result.message;
            setTimeout(() => window.location.href ='/Home/Index', 1000);
        } else {
            msgBox.style.color = "red";
            msgBox.innerText = result.message;
        }
    } catch (err) {
        console.error("Error:", err);
        document.getElementById("loginMessage").style.color = "red";
        document.getElementById("loginMessage").innerText = "Something went wrong!";
    }
});