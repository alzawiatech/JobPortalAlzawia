
// =========================================
// REGISTRATION PAGE - POPUP JAVASCRIPT
// =========================================


// Redirect to Login
function redirectToLogin() {

    const loginElement =
        document.getElementById("loginRedirectUrl");

    if (loginElement && loginElement.value) {

        window.location.href = loginElement.value;

    } else {

        window.location.href = "/Identity/Account/Login";

    }
}


// Close popup and redirect to Login
function closeRegisterToast() {

    const toast =
        document.getElementById("registerToast");

    if (!toast) {

        redirectToLogin();
        return;

    }

    toast.classList.add("toast-hide");

    setTimeout(function () {

        redirectToLogin();

    }, 400);

}


// Automatic popup display and redirect
document.addEventListener("DOMContentLoaded", function () {

    const toast =
        document.getElementById("registerToast");

    if (toast) {

        const isSuccess =
            toast.classList.contains("success-toast");

        // Success popup: 2 seconds
        // Existing email popup: 3 seconds
        const displayTime = isSuccess ? 2000 : 3000;

        setTimeout(function () {

            closeRegisterToast();

        }, displayTime);

    }

});
