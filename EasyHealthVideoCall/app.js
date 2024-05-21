'use strict';

const axios = require('axios');

function makeGetRequest() {
    axios.get('https://api.example.com/data')
        .then(response => {
            console.log(response.data);
        })
        .catch(error => {
            console.error('Error fetching data:', error);
        });
}
makeGetRequest();
require('readline')
    .createInterface(process.stdin, process.stdout)
    .question("Press [Enter] to exit...", function () {
        process.exit();
    });