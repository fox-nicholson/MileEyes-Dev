function TokenManager() {

    this.token = null;

    if (sessionStorage.getItem('token')) {
        this.token = sessionStorage.getItem('token');
    }

    this.setToken = function(token) {
        this.token = token;
    };

    this.resetToken = function() {
        this.token = null;
    };
}

function Home() {
    $('#body-content')
}