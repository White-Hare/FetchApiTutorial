const uri = 'api/v1/Users/Authenticate';


function login() {
    const usernameTextbox = $('#username');
    const passwordTextbox = $('#password');

    const username = usernameTextbox.val().trim();
    const password = passwordTextbox.val().trim();


    if (!username && !password)
        return false;

    const user = {
        username: username,
        password: password
    };

    fetch(uri, {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(user)
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
                Cookies.set('Authorization', data.jwtToken);
                location.href = "index.html";
            }
        })
        .catch(error => {
            console.error('Login Failed.', error);
            passwordTextbox.val('');
        });
}