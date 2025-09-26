document.addEventListener("DOMContentLoaded", () => {
    const studentTableBody = document.getElementById("studentTableBody");
    const searchInput = document.getElementById("searchInput");
    const searchBtn = document.getElementById("searchBtn");
    const insertBtn = document.getElementById("insertBtn");
    const paginationControls = document.getElementById("paginationControls");
    const prevPageBtn = document.getElementById("prevPageBtn");
    const nextPageBtn = document.getElementById("nextPageBtn");

    let currentPage = 1;
    const pageSize = 10;
    let totalCount = 0;
    let isEditing = false;
    let isInserting = false;

    // Main function to load and render students
    async function loadStudents(page = 1) {
        let keyword = searchInput.value.trim();
        currentPage = page;

        try {
            const response = await fetch(`/Home/GetStudents?keyword=${encodeURIComponent(keyword)}&pageNumber=${currentPage}&pageSize=${pageSize}`);
            const data = await response.json();

            if (data.success) {
                renderTable(data.data);
                totalCount = data.totalCount;
                renderPaginationControls();
            } else {
                studentTableBody.innerHTML = `<tr><td colspan="5" class="text-center text-danger">${data.message}</td></tr>`;
            }
        } catch (error) {
            console.error("Error fetching students:", error);
            studentTableBody.innerHTML = `<tr><td colspan="5" class="text-center text-danger">Failed to load data. Please try again.</td></tr>`;
        }
    }

    // Renders the table content based on fetched data
    function renderTable(students) {
        let rows = "";
        if (students.length === 0) {
            rows = `<tr><td colspan="5" class="text-center text-secondary">No students found.</td></tr>`;
        } else {
            students.forEach(s => {
                rows += `
                    <tr data-id="${s.Id}">
                        <td>${s.Id}</td>
                        <td>${s.Name}</td>
                        <td>${s.Age}</td>
                        <td>${s.Email}</td>
                        <td>
                            <button type="button" class="btn btn-sm btn-outline-success rounded-pill fw-semibold edit-btn">Update</button>
                            <button type="button" class="btn btn-sm btn-outline-danger rounded-pill fw-semibold delete-btn">Delete</button>
                        </td>
                    </tr>`;
            });
        }
        studentTableBody.innerHTML = rows;
    }

    // Renders pagination controls
    function renderPaginationControls() {
        paginationControls.innerHTML = '';
        const totalPages = Math.ceil(totalCount / pageSize);

        for (let i = 1; i <= totalPages; i++) {
            const pageButton = document.createElement('button');
            pageButton.textContent = i;
            pageButton.classList.add('btn', 'btn-outline-primary', 'me-2');
            if (i === currentPage) {
                pageButton.classList.add('active');
            }
            pageButton.addEventListener('click', () => {
                loadStudents(i);
            });
            paginationControls.appendChild(pageButton);
        }

        prevPageBtn.disabled = (currentPage === 1);
        nextPageBtn.disabled = (currentPage >= totalPages);
    }

    // Event listener for search button
    searchBtn.addEventListener("click", () => {
        loadStudents(1); // Reset to page 1 on new search
    });

    // Event listeners for pagination
    prevPageBtn.addEventListener("click", () => {
        if (currentPage > 1) {
            loadStudents(currentPage - 1);
        }
    });

    nextPageBtn.addEventListener("click", () => {
        const totalPages = Math.ceil(totalCount / pageSize);
        if (currentPage < totalPages) {
            loadStudents(currentPage + 1);
        }
    });

    // Event listener for Insert button
    insertBtn.addEventListener("click", () => {
        if (isInserting) {
            return;
        }
        isInserting = true;
        const newRow = document.createElement('tr');
        newRow.id = "insertRow";
        newRow.innerHTML = `
            <td>Auto</td>
            <td><input type="text" name="Name" class="form-control fw-semibold" required /></td>
            <td><input type="number" name="Age" class="form-control fw-semibold" required /></td>
            <td><input type="email" name="Email" class="form-control fw-semibold" required /></td>
            <td>
                <button type="button" class="btn btn-success rounded-pill fw-semibold" id="applyInsertBtn">Apply</button>
                <button type="button" class="btn btn-danger rounded-pill fw-semibold" id="cancelInsertBtn">Cancel</button>
            </td>
        `;
        studentTableBody.prepend(newRow);

        document.getElementById('applyInsertBtn').addEventListener('click', applyInsert);
        document.getElementById('cancelInsertBtn').addEventListener('click', cancelInsert);
    });

    async function applyInsert() {
        const name = document.querySelector("#insertRow input[name='Name']").value;
        const age = document.querySelector("#insertRow input[name='Age']").value;
        const email = document.querySelector("#insertRow input[name='Email']").value;

        if (!name || !age || !email) {
            alert("Please fill all fields.");
            return;
        }

        try {
            const response = await fetch('/Home/InsertStudent', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Name: name, Age: age, Email: email })
            });
            const data = await response.json();
            if (data.success) {
                alert(data.message);
                isInserting = false;
                loadStudents(currentPage); // Refresh the table
            } else {
                alert(data.message);
            }
        } catch (error) {
            alert("An error occurred during insertion.");
        }
    }

    function cancelInsert() {
        const insertRow = document.getElementById("insertRow");
        if (insertRow) {
            insertRow.remove();
            isInserting = false;
        }
    }

    // Event delegation for Update and Delete buttons
    studentTableBody.addEventListener('click', (event) => {
        const target = event.target;
        if (target.classList.contains('edit-btn')) {
            handleEditClick(target);
        } else if (target.classList.contains('delete-btn')) {
            handleDeleteClick(target);
        } else if (target.classList.contains('apply-update-btn')) {
            handleApplyUpdateClick(target);
        } else if (target.classList.contains('cancel-update-btn')) {
            handleCancelUpdateClick(target);
        }
    });

    function handleEditClick(button) {
        if (isEditing || isInserting) {
            return;
        }
        isEditing = true;
        const row = button.closest('tr');
        const cells = row.querySelectorAll('td');

        const id = cells[0].textContent;
        const name = cells[1].textContent;
        const age = cells[2].textContent;
        const email = cells[3].textContent;

        cells[1].innerHTML = `<input type="text" name="Name" class="form-control fw-semibold" value="${name}" required />`;
        cells[2].innerHTML = `<input type="number" name="Age" class="form-control fw-semibold" value="${age}" required />`;
        cells[3].innerHTML = `<input type="email" name="Email" class="form-control fw-semibold" value="${email}" required />`;
        cells[4].innerHTML = `
            <button type="button" class="btn btn-success rounded-pill fw-semibold apply-update-btn">Apply</button>
            <button type="button" class="btn btn-danger rounded-pill fw-semibold cancel-update-btn">Cancel</button>
        `;
        row.dataset.originalContent = JSON.stringify({ name: name, age: age, email: email });
    }

    async function handleApplyUpdateClick(button) {
        const row = button.closest('tr');
        const id = row.dataset.id;
        const name = row.querySelector("input[name='Name']").value;
        const age = row.querySelector("input[name='Age']").value;
        const email = row.querySelector("input[name='Email']").value;

        try {
            const response = await fetch('/Home/UpdateStudent', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Id: id, Name: name, Age: age, Email: email })
            });
            const data = await response.json();
            if (data.success) {
                alert(data.message);
                isEditing = false;
                loadStudents(currentPage);
            } else {
                alert(data.message);
            }
        } catch (error) {
            alert("An error occurred during update.");
        }
    }

    function handleCancelUpdateClick(button) {
        const row = button.closest('tr');
        const cells = row.querySelectorAll('td');
        const originalContent = JSON.parse(row.dataset.originalContent);

        cells[1].textContent = originalContent.name;
        cells[2].textContent = originalContent.age;
        cells[3].textContent = originalContent.email;
        cells[4].innerHTML = `
            <button type="button" class="btn btn-sm btn-outline-success rounded-pill fw-semibold edit-btn">Update</button>
            <button type="button" class="btn btn-sm btn-outline-danger rounded-pill fw-semibold delete-btn">Delete</button>
        `;
        isEditing = false;
    }

    async function handleDeleteClick(button) {
        if (!confirm("Are you sure you want to delete this student?")) {
            return;
        }

        const row = button.closest('tr');
        const id = row.dataset.id;

        try {
            const response = await fetch('/Home/DeleteStudent', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: id })
            });
            const data = await response.json();
            if (data.success) {
                alert(data.message);
                loadStudents(currentPage);
            } else {
                alert(data.message);
            }
        } catch (error) {
            alert("An error occurred during deletion.");
        }
    }

    // Initial load
    loadStudents();
});