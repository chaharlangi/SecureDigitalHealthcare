
const CryptoJS = require('crypto-js');

class MyAppEncryptor {
    constructor() {
        this.encryptionKey = CryptoJS.enc.Utf8.parse("7RBOwUpx1cv7VR+Bi3tWyI+QkWwC5NN7");
        this.iv = CryptoJS.lib.WordArray.random(16); // Initialization vector
    }

    getIv() {
        return this.iv.toString();
    }
    getIvBase64() {
        return CryptoJS.enc.Base64.stringify(this.iv)
    }

    encryptString(plainText) {
        const encrypted = CryptoJS.AES.encrypt(plainText, this.encryptionKey, {
            iv: this.iv,
            mode: CryptoJS.mode.CBC,
            padding: CryptoJS.pad.Pkcs7 // Ensure PKCS7 padding
        });

        console.log("Plain text: " + plainText)
        console.log("Encrypted: " + encrypted)
        console.log("Encryption key: " + this.encryptionKey);
        console.log("IV: " + this.getIv() + " length: " + this.getIv().length);
        console.log("IV Base64: " + this.getIvBase64() + " length: " + this.getIvBase64().length);

        return encrypted.toString();
    }

    decryptString(encryptedText) {
        const decrypted = CryptoJS.AES.decrypt(encryptedText, this.encryptionKey, {
            iv: this.iv,
            mode: CryptoJS.mode.CBC,
            padding: CryptoJS.pad.Pkcs7
        });

        return decrypted.toString(CryptoJS.enc.Utf8);
    }
}

function encryptString(plainText) {
    const encrypted = CryptoJS.AES.encrypt(plainText, encryptionKey, { iv });
    return encrypted.toString();
}

function decryptString(encryptedText) {
    const decrypted = CryptoJS.AES.decrypt(encryptedText, encryptionKey, { iv });
    return decrypted.toString(CryptoJS.enc.Utf8);
}

// Exporting functions
module.exports = {
    MyAppEncryptor
};