# 🎯 Quiz Tabelline con Riconoscimento Vocale

Un'applicazione web moderna per l'apprendimento delle tabelline di moltiplicazione con supporto per il riconoscimento vocale, costruita con **ASP.NET Core Blazor Server**.

## ✨ Caratteristiche Principali

### 🎤 **Riconoscimento Vocale**
- Rispondi alle domande con la voce usando la **Web Speech API**
- Supporto per numeri in italiano (es. "ventiquattro", "trentasei")
- Fallback automatico per input da tastiera

### 📊 **Statistiche Avanzate**
- **Accuratezza**: Percentuale di risposte corrette
- **Velocità**: Tempo medio, record personali, distribuzione tempi
- **Analisi tabelline**: Identifica le moltiplicazioni più difficili
- **Trend progresso**: Confronto prima/seconda metà sessione
- **Streak tracking**: Sequenze di risposte corrette consecutive

### 🏆 **Sistema Gamification**
- **Punteggio complessivo**: Combinazione di velocità e accuratezza
- **Livelli**: Da "🚀 IN ALLENAMENTO" a "🏆 ESPERTO"
- **Achievements**: Traguardi sbloccabili (Velocista, Precision Master, etc.)
- **Statistiche visive**: Grafici e progress bar interattivi

### 🎮 **Esperienza Utente**
- **Interfaccia responsive**: Ottimizzata per desktop e mobile
- **Timer in tempo reale**: Countdown visibile
- **Feedback immediato**: Risposte animate con emoji
- **Design moderno**: Bootstrap 5 con tema personalizzato

## 🚀 Come Iniziare

### Prerequisiti
- **.NET 9.0** o superiore
- **Browser moderno** con supporto Web Speech API (Chrome, Edge, Safari)

### Installazione e Avvio

1. **Clona il repository**:
   ```bash
   git clone <repository-url>
   cd TabellineVocali
   ```

2. **Installa le dipendenze**:
   ```bash
   dotnet restore
   ```

3. **Avvia l'applicazione**:
   ```bash
   dotnet run
   ```

4. **Apri il browser** su: `http://localhost:5222`

### Utilizzo in VS Code

1. Apri il progetto in VS Code
2. Usa `Ctrl+Shift+P` > "Tasks: Run Task" > "run-blazor-app"
3. L'applicazione si aprirà automaticamente

## 🎯 Come Giocare

### 1. **Configurazione Sessione**
- Imposta la durata della sessione (1-60 minuti)
- Clicca "🚀 Inizia Quiz"

### 2. **Rispondi alle Domande**
- **Tastiera**: Inserisci il numero e premi Invio o "✅ Conferma"
- **Voce**: Clicca "🎤 Voce" e pronuncia la risposta

### 3. **Comandi Vocali Supportati**
- **Numeri in italiano**: "ventiquattro", "trentasei", "quarantadue"
- **Cifre**: "24", "36", "42"
- **Assicurati** che il microfono sia attivo e autorizzato

### 4. **Visualizza Risultati**
- Al termine, ricevi un riepilogo completo con tutte le statistiche
- Identifica le aree di miglioramento
- Sblocca achievements in base alle prestazioni

## 🏗️ Architettura del Progetto

```
TabellineVocali/
├── Components/
│   ├── Pages/
│   │   ├── Home.razor              # Pagina iniziale
│   │   ├── QuizComponent.razor     # Componente principale quiz
│   │   └── SessionResults.razor    # Riepilogo risultati
│   └── Layout/
│       ├── MainLayout.razor        # Layout principale
│       └── NavMenu.razor           # Menu navigazione
├── Models/
│   └── QuizSession.cs              # Modelli dati
├── Services/
│   └── QuizService.cs              # Logica business quiz
├── wwwroot/
│   └── js/
│       └── voice-recognition.js    # JavaScript per riconoscimento vocale
└── Program.cs                      # Configurazione app
```

### Tecnologie Utilizzate

- **Backend**: ASP.NET Core 9.0, Blazor Server
- **Frontend**: Bootstrap 5, CSS personalizzato
- **JavaScript**: Web Speech API per riconoscimento vocale
- **Pattern**: Service-oriented architecture, Dependency Injection

## 📊 Statistiche Implementate

### **Metriche Base**
- Risposte corrette/sbagliate
- Accuratezza percentuale
- Tentativi totali ed errati

### **Analisi Velocità**
- Tempo medio per risposta
- Record personale (tempo minimo)
- Tempo massimo impiegato
- Ritmo (risposte al minuto)

### **Distribuzione Temporale**
- Risposte sotto 1 secondo
- Risposte 1-2 secondi
- Risposte 2-3 secondi  
- Risposte sopra 3 secondi

### **Analisi Avanzata**
- **Tabelline difficili**: Quali moltiplicazioni causano più errori
- **Tabelline veloci**: Quali vengono risolte più rapidamente
- **Streak massimo**: Sequenza più lunga di risposte corrette
- **Trend sessione**: Miglioramento/peggioramento durante il gioco

### **Sistema Punteggi**
- **Formula**: 40% velocità + 60% accuratezza
- **Livelli**: 5 categorie da Principiante a Esperto
- **Achievements**: 6 traguardi sbloccabili

## 🏅 Achievements Disponibili

| Achievement | Descrizione | Condizione |
|-------------|-------------|------------|
| 🔥 **STREAK MASTER** | Maestro delle sequenze | 10+ risposte consecutive corrette |
| ⚡ **VELOCISTA** | Velocità supersonica | Tempo medio sotto 1.5 secondi |
| 🎯 **PRECISION MASTER** | Precisione perfetta | 95%+ di accuratezza |
| 💨 **SPEED DEMON** | Demone della velocità | 5+ risposte sotto 1 secondo |
| 💪 **ENDURANCE** | Resistenza | 20+ domande completate |
| 🎤 **VOICE MASTER** | Maestro vocale | Utilizzato riconoscimento vocale |

## 🌐 Compatibilità Browser

| Browser | Supporto Riconoscimento Vocale | Note |
|---------|-------------------------------|------|
| **Chrome** | ✅ Completo | Raccomandato |
| **Edge** | ✅ Completo | Ottimo supporto |
| **Safari** | ✅ Parziale | iOS 14.5+ |
| **Firefox** | ❌ Non supportato | Solo input tastiera |

## 🚀 Funzionalità Future

- [ ] **Modalità multiplayer** con sfide in tempo reale
- [ ] **Profili utente** con storico progressi
- [ ] **Difficoltà personalizzabile** (range numeri, operazioni)
- [ ] **Modalità allenamento** per tabelline specifiche
- [ ] **Esportazione statistiche** (PDF, Excel)
- [ ] **Temi personalizzati** e modalità scura
- [ ] **Supporto offline** con PWA

## 🤝 Contribuire

1. Fai fork del repository
2. Crea un branch per la feature (`git checkout -b feature/AmazingFeature`)
3. Commit le modifiche (`git commit -m 'Add AmazingFeature'`)
4. Push al branch (`git push origin feature/AmazingFeature`)
5. Apri una Pull Request

## 📝 Licenza

Questo progetto è sotto licenza MIT. Vedi il file `LICENSE` per dettagli.

## 📞 Supporto

Per domande, bug report o suggerimenti:
- Apri una **Issue** su GitHub
- Contatta il team di sviluppo

---

**Made with ❤️ and 🎯 for mathematics education**