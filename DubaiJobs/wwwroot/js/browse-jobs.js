document.addEventListener("DOMContentLoaded", function () {

    const jobList = document.getElementById("jobList");
    const jobCount = document.getElementById("jobCount");
    const noJobs = document.getElementById("noJobs");

    const keywordSearch = document.getElementById("keywordSearch");
    const locationSearch = document.getElementById("locationSearch");
    const searchButton = document.getElementById("searchButton");

    const categoryFilter = document.getElementById("categoryFilter");
    const minSalary = document.getElementById("minSalary");
    const maxSalary = document.getElementById("maxSalary");

    const applyFilters = document.getElementById("applyFilters");
    const clearFilters = document.getElementById("clearFilters");
    const sortJobs = document.getElementById("sortJobs");

    const filterSidebar = document.getElementById("filterSidebar");
    const openFilters = document.getElementById("openFilters");
    const closeFilters = document.getElementById("closeFilters");

    const jobDetailPanel = document.getElementById("jobDetailPanel");
    const detailPlaceholder = document.getElementById("detailPlaceholder");
    const detailContent = document.getElementById("detailContent");

    const mobileDetailOverlay = document.getElementById("mobileDetailOverlay");
    const mobileDetailContent = document.getElementById("mobileDetailContent");
    const closeJobDetails = document.getElementById("closeJobDetails");

    const pagination = document.getElementById("jobPagination");
    const pageNumbers = document.getElementById("pageNumbers");
    const prevPage = document.getElementById("prevPage");
    const nextPage = document.getElementById("nextPage");
    const paginationInfo = document.getElementById("paginationInfo");

    // Pagination settings
    const jobsPerPage = 6;
    let currentPage = 1;

    // Keep the original job cards for filtering and sorting.
    const allJobs = Array.from(
        jobList.querySelectorAll(".job-item")
    );

    let filteredJobs = [...allJobs];

    // --------------------------------------------------
    // HELPER FUNCTIONS
    // --------------------------------------------------

    function getValue(element) {
        return element ? element.value.trim().toLowerCase() : "";
    }

    function getCheckedFilters(filterName) {
        return Array.from(
            document.querySelectorAll(
                `.job-filter[data-filter="${filterName}"]:checked`
            )
        ).map(input => input.value.toLowerCase());
    }

    function getSalary(job, attribute) {
        const value = parseFloat(job.dataset[attribute]);
        return Number.isFinite(value) ? value : 0;
    }

    function isMobileLayout() {
        return window.matchMedia("(max-width: 1250px)").matches;
    }

    function closeMobileDetails() {
        mobileDetailOverlay.classList.remove("open");
        document.body.style.overflow = "";
    }

    // --------------------------------------------------
    // OPEN JOB DETAILS
    // --------------------------------------------------

    function openJobDetails(job) {

        if (!job) return;

        // Highlight the selected job.
        allJobs.forEach(item => {
            item.classList.remove("selected");
        });

        job.classList.add("selected");

        const template = job.querySelector(".job-details-template");

        if (!template) return;

        const detailsHTML = template.innerHTML;

        if (isMobileLayout()) {
            mobileDetailContent.innerHTML = detailsHTML;
            mobileDetailOverlay.classList.add("open");
            document.body.style.overflow = "hidden";
        } else {
            detailContent.innerHTML = detailsHTML;

            if (detailPlaceholder) {
                detailPlaceholder.style.display = "none";
            }

            jobDetailPanel.scrollTop = 0;
        }
    }

    // Use one delegated click handler for the job cards.
    jobList.addEventListener("click", function (event) {

        const job = event.target.closest(".job-item");

        if (!job) return;

        // Allow links inside the job card to work normally.
        if (event.target.closest("a")) return;

        openJobDetails(job);
    });

    // Close mobile job details.
    if (closeJobDetails) {
        closeJobDetails.addEventListener("click", closeMobileDetails);
    }

    // --------------------------------------------------
    // FILTER JOBS
    // --------------------------------------------------

    function filterJobs() {

        const keyword = getValue(keywordSearch);
        const location = getValue(locationSearch);
        const category = getValue(categoryFilter);

        const selectedEmployment = getCheckedFilters("employment");
        const selectedWorkplace = getCheckedFilters("workplace");

        const minimumSalary = minSalary.value.trim() !== ""
            ? parseFloat(minSalary.value)
            : null;

        const maximumSalary = maxSalary.value.trim() !== ""
            ? parseFloat(maxSalary.value)
            : null;

        filteredJobs = allJobs.filter(job => {

            const title = (job.dataset.title || "").toLowerCase();
            const company = (job.dataset.company || "").toLowerCase();
            const jobLocation = (job.dataset.location || "").toLowerCase();
            const jobEmployment = (job.dataset.employment || "").toLowerCase();
            const jobWorkplace = (job.dataset.workplace || "").toLowerCase();
            const jobCategory = (job.dataset.category || "").toLowerCase();

            const description = (
                job.querySelector(".job-description-preview")?.textContent || ""
            ).toLowerCase();

            const skills = Array.from(
                job.querySelectorAll(".tag-skill")
            ).map(skill => skill.textContent.toLowerCase()).join(" ");

            // Keyword search
            const matchesKeyword =
                !keyword ||
                title.includes(keyword) ||
                company.includes(keyword) ||
                jobCategory.includes(keyword) ||
                description.includes(keyword) ||
                skills.includes(keyword);

            // Location search
            const matchesLocation =
                !location || jobLocation.includes(location);

            // Category search
            const matchesCategory =
                !category || jobCategory.includes(category);

            // Employment type
            const matchesEmployment =
                selectedEmployment.length === 0 ||
                selectedEmployment.includes(jobEmployment);

            // Workplace
            const matchesWorkplace =
                selectedWorkplace.length === 0 ||
                selectedWorkplace.includes(jobWorkplace);

            // Salary range
            const jobMinimum = getSalary(job, "minsalary");
            const jobMaximum = getSalary(job, "maxsalary");

            let matchesSalary = true;

            if (minimumSalary !== null && !Number.isNaN(minimumSalary)) {
                matchesSalary = matchesSalary &&
                    jobMaximum >= minimumSalary;
            }

            if (maximumSalary !== null && !Number.isNaN(maximumSalary)) {
                matchesSalary = matchesSalary &&
                    jobMinimum <= maximumSalary;
            }

            return matchesKeyword &&
                matchesLocation &&
                matchesCategory &&
                matchesEmployment &&
                matchesWorkplace &&
                matchesSalary;
        });

        sortFilteredJobs();

        currentPage = 1;
        renderJobs();
    }

    // --------------------------------------------------
    // SORT JOBS
    // --------------------------------------------------

    function sortFilteredJobs() {

        const sortValue = sortJobs ? sortJobs.value : "recent";

        if (sortValue === "salaryHigh") {

            filteredJobs.sort((a, b) => {
                return getSalary(b, "maxsalary") -
                    getSalary(a, "maxsalary");
            });

        } else if (sortValue === "salaryLow") {

            filteredJobs.sort((a, b) => {
                return getSalary(a, "minsalary") -
                    getSalary(b, "minsalary");
            });

        } else {

            filteredJobs.sort((a, b) => {
                const dateA = new Date(a.dataset.posted || 0);
                const dateB = new Date(b.dataset.posted || 0);

                return dateB - dateA;
            });
        }
    }

    // --------------------------------------------------
    // PAGINATION
    // --------------------------------------------------

    function renderPagination() {

        pageNumbers.innerHTML = "";

        const totalPages = Math.ceil(
            filteredJobs.length / jobsPerPage
        );

        if (totalPages <= 1) {
            pagination.hidden = true;
            paginationInfo.hidden = true;
            return;
        }

        pagination.hidden = false;
        paginationInfo.hidden = false;

        prevPage.disabled = currentPage === 1;
        nextPage.disabled = currentPage === totalPages;

        // Show page numbers.
        for (let page = 1; page <= totalPages; page++) {

            const button = document.createElement("button");

            button.type = "button";
            button.className = "page-number";
            button.textContent = page;
            button.setAttribute("aria-label", `Go to page ${page}`);

            if (page === currentPage) {
                button.classList.add("active");
                button.setAttribute("aria-current", "page");
            }

            button.addEventListener("click", function () {
                currentPage = page;
                renderJobs();
                scrollToJobList();
            });

            pageNumbers.appendChild(button);
        }

        const start = (currentPage - 1) * jobsPerPage + 1;
        const end = Math.min(
            currentPage * jobsPerPage,
            filteredJobs.length
        );

        paginationInfo.textContent =
            `Showing ${start}–${end} of ${filteredJobs.length} jobs`;
    }

    function scrollToJobList() {

        const section = document.querySelector(".job-list-section");

        if (section) {
            section.scrollIntoView({
                behavior: "smooth",
                block: "start"
            });
        }
    }

    // --------------------------------------------------
    // DISPLAY JOBS
    // --------------------------------------------------

    function renderJobs() {

        // Hide all job cards first.
        allJobs.forEach(job => {
            job.style.display = "none";
        });

        const totalJobs = filteredJobs.length;
        const totalPages = Math.ceil(totalJobs / jobsPerPage);

        if (totalJobs === 0) {

            jobCount.textContent = "0";
            noJobs.style.display = "block";

            pagination.hidden = true;
            paginationInfo.hidden = true;

            // Clear selected details.
            detailContent.innerHTML = "";

            if (detailPlaceholder) {
                detailPlaceholder.style.display = "flex";
            }

            closeMobileDetails();
            return;
        }

        noJobs.style.display = "none";

        // Keep current page within valid range.
        if (currentPage > totalPages) {
            currentPage = totalPages;
        }

        const startIndex = (currentPage - 1) * jobsPerPage;
        const endIndex = startIndex + jobsPerPage;

        const currentJobs = filteredJobs.slice(startIndex, endIndex);

        currentJobs.forEach(job => {
            job.style.display = "";
            jobList.appendChild(job);
        });

        jobCount.textContent = totalJobs;

        renderPagination();

        // Automatically select the first visible job on desktop.
        if (!isMobileLayout() && currentJobs.length > 0) {

            const selectedJob = currentJobs.find(job =>
                job.classList.contains("selected")
            );

            openJobDetails(selectedJob || currentJobs[0]);
        }
    }

    // --------------------------------------------------
    // SEARCH BUTTON
    // --------------------------------------------------

    if (searchButton) {
        searchButton.addEventListener("click", function () {
            filterJobs();
        });
    }

    // Search when pressing Enter.
    [keywordSearch, locationSearch].forEach(input => {

        if (!input) return;

        input.addEventListener("keydown", function (event) {

            if (event.key === "Enter") {
                event.preventDefault();
                filterJobs();
            }
        });
    });

    // --------------------------------------------------
    // APPLY FILTERS
    // --------------------------------------------------

    if (applyFilters) {
        applyFilters.addEventListener("click", function () {

            filterJobs();

            if (window.innerWidth <= 1100) {
                filterSidebar.classList.remove("open");
            }
        });
    }

    // --------------------------------------------------
    // CLEAR FILTERS
    // --------------------------------------------------

    if (clearFilters) {
        clearFilters.addEventListener("click", function () {

            if (keywordSearch) keywordSearch.value = "";
            if (locationSearch) locationSearch.value = "";
            if (categoryFilter) categoryFilter.value = "";
            if (minSalary) minSalary.value = "";
            if (maxSalary) maxSalary.value = "";

            document.querySelectorAll(".job-filter").forEach(input => {
                input.checked = false;
            });

            if (sortJobs) {
                sortJobs.value = "recent";
            }

            currentPage = 1;
            filterJobs();

            if (window.innerWidth <= 1100) {
                filterSidebar.classList.remove("open");
            }
        });
    }

    // --------------------------------------------------
    // SORT CHANGE
    // --------------------------------------------------

    if (sortJobs) {
        sortJobs.addEventListener("change", function () {

            sortFilteredJobs();
            currentPage = 1;
            renderJobs();
        });
    }

    // --------------------------------------------------
    // PREVIOUS / NEXT PAGE
    // --------------------------------------------------

    if (prevPage) {
        prevPage.addEventListener("click", function () {

            if (currentPage > 1) {
                currentPage--;
                renderJobs();
                scrollToJobList();
            }
        });
    }

    if (nextPage) {
        nextPage.addEventListener("click", function () {

            const totalPages = Math.ceil(
                filteredJobs.length / jobsPerPage
            );

            if (currentPage < totalPages) {
                currentPage++;
                renderJobs();
                scrollToJobList();
            }
        });
    }

    // --------------------------------------------------
    // MOBILE FILTER OPEN / CLOSE
    // --------------------------------------------------

    if (openFilters) {
        openFilters.addEventListener("click", function () {
            filterSidebar.classList.add("open");
        });
    }

    if (closeFilters) {
        closeFilters.addEventListener("click", function () {
            filterSidebar.classList.remove("open");
        });
    }

    // --------------------------------------------------
    // WINDOW RESIZE
    // --------------------------------------------------

    window.addEventListener("resize", function () {

        // Close the mobile details overlay when returning to desktop.
        if (!isMobileLayout()) {
            closeMobileDetails();
        }
    });

    // --------------------------------------------------
    // INITIAL PAGE LOAD
    // --------------------------------------------------

    sortFilteredJobs();
    renderJobs();

});