const authenticateUri = 'api/v1/Users/Authenticate';

function refresh(username, password) {

}

function logout() {
    fetch(`${authenticateUri}/Revoke`, {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else
                throw Error(response);
        })
        .then(data => {
            console.log(data);
            if (data.jwtToken) {
                location.href = "index.html";
            }
        })
        .catch(error => {
            console.error('Logout Failed.', error);
        });
}