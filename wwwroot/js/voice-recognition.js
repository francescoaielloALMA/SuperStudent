// Voice Recognition Service
window.voiceRecognition = {
    recognition: null,
    isSupported: false,
    isListening: false,

    initialize: function() {
        // Check if browser supports speech recognition
        this.isSupported = 'webkitSpeechRecognition' in window || 'SpeechRecognition' in window;
        
        if (this.isSupported) {
            const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
            this.recognition = new SpeechRecognition();
            
            // Configure recognition
            this.recognition.continuous = false;
            this.recognition.interimResults = false;
            this.recognition.lang = 'it-IT'; // Italian language
            this.recognition.maxAlternatives = 1;
            
            console.log('Voice recognition initialized');
        } else {
            console.warn('Browser does not support speech recognition');
        }
        
        return this.isSupported;
    },

    startListening: function(dotnetHelper) {
        if (!this.isSupported || !this.recognition) {
            console.error('Voice recognition not supported or not initialized');
            return false;
        }

        if (this.isListening) {
            console.warn('Already listening');
            return false;
        }

        this.isListening = true;
        
        this.recognition.onstart = () => {
            console.log('Voice recognition started');
            dotnetHelper.invokeMethodAsync('OnVoiceStart');
        };

        this.recognition.onresult = (event) => {
            const result = event.results[0][0].transcript;
            console.log('Voice recognition result:', result);
            
            // Try to extract number from speech
            const number = this.extractNumber(result);
            if (number !== null) {
                dotnetHelper.invokeMethodAsync('OnVoiceResult', number.toString());
            } else {
                dotnetHelper.invokeMethodAsync('OnVoiceError', 'Non ho capito il numero. Riprova.');
            }
        };

        this.recognition.onerror = (event) => {
            console.error('Voice recognition error:', event.error);
            this.isListening = false;
            
            let errorMessage = 'Errore nel riconoscimento vocale';
            switch(event.error) {
                case 'no-speech':
                    errorMessage = 'Nessun audio rilevato. Riprova.';
                    break;
                case 'audio-capture':
                    errorMessage = 'Microfono non disponibile.';
                    break;
                case 'not-allowed':
                    errorMessage = 'Permesso microfono negato.';
                    break;
                case 'network':
                    errorMessage = 'Errore di rete.';
                    break;
            }
            
            dotnetHelper.invokeMethodAsync('OnVoiceError', errorMessage);
        };

        this.recognition.onend = () => {
            console.log('Voice recognition ended');
            this.isListening = false;
            dotnetHelper.invokeMethodAsync('OnVoiceEnd');
        };

        try {
            this.recognition.start();
            return true;
        } catch (error) {
            console.error('Error starting voice recognition:', error);
            this.isListening = false;
            return false;
        }
    },

    stopListening: function() {
        if (this.recognition && this.isListening) {
            this.recognition.stop();
            this.isListening = false;
            console.log('Voice recognition stopped');
        }
    },

    extractNumber: function(text) {
        // Remove accents and convert to lowercase
        const normalizedText = text.toLowerCase()
            .replace(/à/g, 'a').replace(/è/g, 'e').replace(/ì/g, 'i')
            .replace(/ò/g, 'o').replace(/ù/g, 'u');

        // Italian number words mapping
        const numberWords = {
            'zero': 0, 'uno': 1, 'due': 2, 'tre': 3, 'quattro': 4,
            'cinque': 5, 'sei': 6, 'sette': 7, 'otto': 8, 'nove': 9,
            'dieci': 10, 'undici': 11, 'dodici': 12, 'tredici': 13,
            'quattordici': 14, 'quindici': 15, 'sedici': 16, 'diciassette': 17,
            'diciotto': 18, 'diciannove': 19, 'venti': 20, 'ventuno': 21,
            'ventidue': 22, 'ventitré': 23, 'ventiquattro': 24, 'venticinque': 25,
            'ventisei': 26, 'ventisette': 27, 'ventotto': 28, 'ventinove': 29,
            'trenta': 30, 'trentuno': 31, 'trentadue': 32, 'trentatré': 33,
            'trentaquattro': 34, 'trentacinque': 35, 'trentasei': 36,
            'trentasette': 37, 'trentotto': 38, 'trentanove': 39,
            'quaranta': 40, 'quarantuno': 41, 'quarantadue': 42, 'quarantatré': 43,
            'quarantaquattro': 44, 'quarantacinque': 45, 'quarantasei': 46,
            'quarantasette': 47, 'quarantotto': 48, 'quarantanove': 49,
            'cinquanta': 50, 'cinquantuno': 51, 'cinquantadue': 52,
            'cinquantatré': 53, 'cinquantaquattro': 54, 'cinquantacinque': 55,
            'cinquantasei': 56, 'cinquantasette': 57, 'cinquantotto': 58,
            'cinquantanove': 59, 'sessanta': 60, 'sessantuno': 61,
            'sessantadue': 62, 'sessantatré': 63, 'sessantaquattro': 64,
            'sessantacinque': 65, 'sessantasei': 66, 'sessantasette': 67,
            'sessantotto': 68, 'sessantanove': 69, 'settanta': 70,
            'settantuno': 71, 'settantadue': 72, 'settantatré': 73,
            'settantaquattro': 74, 'settantacinque': 75, 'settantasei': 76,
            'settantasette': 77, 'settantotto': 78, 'settantanove': 79,
            'ottanta': 80, 'ottantuno': 81
        };

        // First try to find direct number match
        for (const [word, number] of Object.entries(numberWords)) {
            if (normalizedText.includes(word)) {
                return number;
            }
        }

        // Try to extract digits
        const digitMatch = normalizedText.match(/\d+/);
        if (digitMatch) {
            return parseInt(digitMatch[0]);
        }

        return null;
    }
};

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    window.voiceRecognition.initialize();
});