
document.addEventListener("DOMContentLoaded", function () {

    // Sticky Header
    const header = document.getElementById("header");

    if (header) {
        window.addEventListener("scroll", function () {
            header.classList.toggle("scrolled", window.scrollY > 40);
        });
    }

    // Mobile Navigation
    const hamburger = document.getElementById("hamburger");
    const mobileNav = document.getElementById("mobile-nav");

    if (hamburger && mobileNav) {

        hamburger.addEventListener("click", function () {

            const isOpen = mobileNav.classList.toggle("open");

            hamburger.classList.toggle("open", isOpen);

            hamburger.setAttribute(
                "aria-expanded",
                isOpen ? "true" : "false"
            );

            document.body.style.overflow = isOpen ? "hidden" : "";

        });

        // Close mobile menu after clicking a link
        mobileNav.querySelectorAll("a").forEach(function (link) {

            link.addEventListener("click", function () {

                mobileNav.classList.remove("open");
                hamburger.classList.remove("open");

                hamburger.setAttribute("aria-expanded", "false");

                document.body.style.overflow = "";

            });

        });

        // Close mobile menu when clicking outside
        document.addEventListener("click", function (event) {

            if (
                !header.contains(event.target) &&
                mobileNav.classList.contains("open")
            ) {

                mobileNav.classList.remove("open");
                hamburger.classList.remove("open");

                hamburger.setAttribute("aria-expanded", "false");

                document.body.style.overflow = "";

            }

        });

    }

    // Scroll To Top
    const scrollTopBtn = document.getElementById("scroll-top");

    if (scrollTopBtn) {

        window.addEventListener("scroll", function () {

            scrollTopBtn.classList.toggle(
                "visible",
                window.scrollY > 400
            );

        });

        scrollTopBtn.addEventListener("click", function () {

            window.scrollTo({
                top: 0,
                behavior: "smooth"
            });

        });

    }

});
