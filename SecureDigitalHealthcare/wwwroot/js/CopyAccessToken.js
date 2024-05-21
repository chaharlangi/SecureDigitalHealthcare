function copyText() {
    var copyText = document.getElementById("textToCopy");

    // Use the Clipboard API

    navigator.clipboard.writeText(copyText.value).then(function () {
        //alert('Copied to clipboard: ' + copyText.value);
    }).catch(function (err) {
        console.error('Could not copy text: ', err);
    });
}