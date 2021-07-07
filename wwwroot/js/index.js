const indexUri = 'api/v1/Tasks';


const headers = {
    'Accept': 'application/json',
    'Content-Type': 'application/json',
    //'Authorization': 'Bearer: ' + Cookies.get('Authorization')
}

let tasks = [];


function getItems() {
    fetch(indexUri,
            {
                method: 'GET',
                headers: headers
            })
        .then(response => response.json())
        .then(data => _displayItems(data))
        .catch(error => {
            console.error('Unable to get items.', error);
            _goLoginPage();
        });
}

function addItem() {
    const addIdHidden = $('#add-id');
    const addTitleTextbox = $('#add-title');
    const addContentTextbox = $('#add-content');

    const itemTitle = addTitleTextbox.val().trim();
    const itemContent = addContentTextbox.val().trim();


    if (!itemTitle && !itemContent)
        return false;

    const item = {
        title: itemTitle,
        content: itemContent
    };

    fetch(indexUri,
            {
                method: 'POST',
                headers: headers,
                body: JSON.stringify(item)
            })
        .then(response => {
            response.json();
        })
        .then(() => {
            getItems();
            addIdHidden.val('');
            addTitleTextbox.val('');
            addContentTextbox.val('');
        })
        .catch(error => {
            console.error('Unable to add item.', error);
            _goLoginPage();
        });
}

function deleteItem(id) {
    fetch(`${indexUri}/${id}`,
            {
                method: 'DELETE',
                headers: headers
            })
        .then(() => getItems())
        .catch(error => {
            console.error('Unable to delete item.', error);
            _goLoginPage();
        });
};

function displayEditForm(id) {
    const item = tasks.find(item => item.id === id);

    $('#edit-id').val(item.id);
    $('#edit-title').val(item.title);
    $('#edit-content').val(item.content);
    $('#editForm').css('display', 'block');
}

function updateItem() {
    const itemId = document.getElementById('edit-id').value.trim();
    const itemTitle = document.getElementById('edit-title').value.trim();
    const itemContent = document.getElementById('edit-content').value.trim();

    if (!itemId && !itemTitle && !itemContent)
        return false;

    const item = {
        title: itemTitle,
        content: itemContent
    };

    fetch(`${indexUri}/${itemId}`, {
        method: 'PUT',
        headers: headers,
        body: JSON.stringify(item)
    })
        .then(() => getItems())
        .catch(error => {
            console.error('Unable to update item.', error);
            _goLoginPage();
        });
        

    closeInput();

    return false;
}

function closeInput() {
    document.getElementById('editForm').style.display = 'none';
}

function _displayCount(itemCount) {

    document.getElementById('counter').innerText = `${itemCount} Görev Var`;
}

function _displayItems(data) {
    const tBody = document.getElementById('tasks');
    tBody.innerHTML = '';

    _displayCount(data.length);

    const button = document.createElement('button');

    data.forEach(item => {
        let editButton = button.cloneNode(false);
        editButton.innerText = 'Düzenle';
        editButton.setAttribute('onclick', `displayEditForm("${item.id}")`);
        editButton.classList.add("btn");
        editButton.classList.add("btn-dark");
        editButton.setAttribute("scope", "row");;


        let deleteButton = button.cloneNode(false);
        deleteButton.innerText = 'Sil';
        deleteButton.setAttribute('onclick', `deleteItem("${item.id}")`);
        deleteButton.classList.add("btn");
        deleteButton.classList.add("btn-dark");
        deleteButton.setAttribute("scope", "row");


        let tr = tBody.insertRow();


        let td2 = tr.insertCell(0);
        let textNode = document.createTextNode(item.title);
        td2.appendChild(textNode);

        let td3 = tr.insertCell(1);
        textNode = document.createTextNode(item.content);
        td3.appendChild(textNode);

        let td4 = tr.insertCell(2);
        td4.appendChild(editButton);

        let td5 = tr.insertCell(3);
        td5.appendChild(deleteButton);
    });

    tasks = data;
}

function _goLoginPage() {
    location.href = "login.html";
}