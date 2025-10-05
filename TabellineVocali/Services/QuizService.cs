using TabellineVocali.Models;

namespace TabellineVocali.Services;

public class QuizService
{
    private readonly Random _random = new();
    private readonly Dictionary<string, QuizSession> _sessions = new();
    private string _sessionId = Guid.NewGuid().ToString();

    public QuizSession? CurrentSession => _sessions.TryGetValue(_sessionId, out var session) ? session : null;
    public bool HasActiveSession => CurrentSession?.IsActive == true;

    public QuizSession StartNewSession(int durataMinuti)
    {
        var session = new QuizSession
        {
            DurataMinuti = durataMinuti,
            InizioSessione = DateTime.Now,
            IsActive = true
        };
        _sessions[_sessionId] = session;
        return session;
    }

    public void EndSession()
    {
        if (_sessions.TryGetValue(_sessionId, out var session))
        {
            session.IsActive = false;
        }
    }

    public (int a, int b) GenerateQuestion()
    {
        var currentSession = CurrentSession;
        if (currentSession == null) throw new InvalidOperationException("Nessuna sessione attiva");

        int a, b;
        string chiaveTabellina;
        
        // Evita domande duplicate consecutive
        do
        {
            a = _random.Next(2, 10);
            b = _random.Next(2, 10);
            chiaveTabellina = $"{a}x{b}";
        } while (chiaveTabellina == currentSession.UltimaDomanda && currentSession.Risposte.Any());

        currentSession.UltimaDomanda = chiaveTabellina;
        return (a, b);
    }

    public bool SubmitAnswer(int a, int b, int risposta, double tempoRisposta, int tentativi, bool usedVoice = false)
    {
        var currentSession = CurrentSession;
        if (currentSession == null) throw new InvalidOperationException("Nessuna sessione attiva");

        var rispostaCorretta = a * b;
        var isCorretta = risposta == rispostaCorretta;
        var chiaveTabellina = $"{a}x{b}";

        var questionResult = new QuestionResult
        {
            A = a,
            B = b,
            RispostaCorretta = rispostaCorretta,
            RispostaUtente = risposta,
            IsCorretta = isCorretta,
            TempoRisposta = tempoRisposta,
            TentativiEffettuati = tentativi,
            Timestamp = DateTime.Now,
            UsedVoice = usedVoice
        };

        currentSession.Risposte.Add(questionResult);

        if (isCorretta)
        {
            // Aggiorna statistiche per risposta corretta
            if (!currentSession.TempiPerTabellina.ContainsKey(chiaveTabellina))
                currentSession.TempiPerTabellina[chiaveTabellina] = new List<double>();
            
            currentSession.TempiPerTabellina[chiaveTabellina].Add(tempoRisposta);
            
            if (tempoRisposta < currentSession.TempoMinimo) 
                currentSession.TempoMinimo = tempoRisposta;
            if (tempoRisposta > currentSession.TempoMassimo) 
                currentSession.TempoMassimo = tempoRisposta;
            
            currentSession.StreakCorrente++;
            if (currentSession.StreakCorrente > currentSession.StreakMassimo)
                currentSession.StreakMassimo = currentSession.StreakCorrente;
        }
        else
        {
            // Aggiorna statistiche per risposta sbagliata
            if (!currentSession.ErroriPerTabellina.ContainsKey(chiaveTabellina))
                currentSession.ErroriPerTabellina[chiaveTabellina] = 0;
            
            currentSession.ErroriPerTabellina[chiaveTabellina]++;
            currentSession.StreakCorrente = 0;
        }

        return isCorretta;
    }

    public SessionStats CalculateStats()
    {
        var currentSession = CurrentSession;
        if (currentSession == null) return new SessionStats();

        var totaleDomande = currentSession.Corrette + currentSession.Sbagliate;
        var accuratezza = totaleDomande > 0 ? currentSession.Corrette * 100.0 / totaleDomande : 0;
        
        var risposteValide = currentSession.Risposte.Where(r => r.TempoRisposta > 0);
        var tempoMedio = risposteValide.Any() ? risposteValide.Average(r => r.TempoRisposta) : 0;
        var tempoMinimo = risposteValide.Any() ? risposteValide.Min(r => r.TempoRisposta) : 0;
        var tempoMassimo = risposteValide.Any() ? risposteValide.Max(r => r.TempoRisposta) : 0;
        
        var stats = new SessionStats
        {
            Corrette = currentSession.Corrette,
            Sbagliate = currentSession.Sbagliate,
            TentativiTotali = currentSession.TentativiTotali,
            TentativiErrati = currentSession.TentativiErrati,
            AccuratezzaPercentuale = accuratezza,
            TempoMedio = tempoMedio,
            TempoMinimo = tempoMinimo,
            TempoMassimo = tempoMassimo,
            StreakMassimo = currentSession.StreakMassimo
        };

        // Calcola punteggio complessivo
        var punteggioVelocita = stats.TempoMedio > 0 ? Math.Max(0, 100 - (stats.TempoMedio - 1) * 20) : 0;
        stats.PunteggioComplessivo = (punteggioVelocita * 0.4 + stats.AccuratezzaPercentuale * 0.6);

        // Determina livello
        stats.Livello = stats.PunteggioComplessivo >= 80 ? "🏆 ESPERTO" :
                        stats.PunteggioComplessivo >= 65 ? "⭐ AVANZATO" :
                        stats.PunteggioComplessivo >= 45 ? "📈 INTERMEDIO" :
                        stats.PunteggioComplessivo >= 25 ? "🌱 PRINCIPIANTE" : "🚀 IN ALLENAMENTO";

        // Calcola achievements
        stats.Achievements = CalculateAchievements(stats);

        // Tabelline più difficili
        stats.TabellePiuDifficili = currentSession.ErroriPerTabellina
            .OrderByDescending(x => x.Value)
            .Take(3)
            .ToList();

        // Tabelline più veloci
        stats.TabelleVeloci = currentSession.TempiPerTabellina
            .Where(x => x.Value.Any())
            .OrderBy(x => x.Value.Average())
            .Take(3)
            .Select(x => new KeyValuePair<string, double>(x.Key, x.Value.Average()))
            .ToList();

        // Distribuzione tempi
        var risposteCorrette = currentSession.Risposte.Where(r => r.IsCorretta);
        stats.DistribuzioneTempo = new DistribuzioneTempo
        {
            Sotto1s = risposteCorrette.Count(r => r.TempoRisposta < 1.0),
            Da1a2s = risposteCorrette.Count(r => r.TempoRisposta >= 1.0 && r.TempoRisposta < 2.0),
            Da2a3s = risposteCorrette.Count(r => r.TempoRisposta >= 2.0 && r.TempoRisposta < 3.0),
            Sopra3s = risposteCorrette.Count(r => r.TempoRisposta >= 3.0)
        };

        // Trend accuratezza (prima metà vs seconda metà)
        var metaSessione = currentSession.Risposte.Count / 2;
        if (metaSessione > 0)
        {
            var accuratezzaPrimaMeta = currentSession.Risposte.Take(metaSessione).Count(r => r.IsCorretta) * 100.0 / metaSessione;
            var accuratezzaSecondaMeta = currentSession.Risposte.Skip(metaSessione).Count(r => r.IsCorretta) * 100.0 / (currentSession.Risposte.Count - metaSessione);
            stats.TrendAccuratezza = accuratezzaSecondaMeta - accuratezzaPrimaMeta;
        }

        return stats;
    }

    private List<string> CalculateAchievements(SessionStats stats)
    {
        var achievements = new List<string>();
        var currentSession = CurrentSession;

        if (currentSession == null) return achievements;

        if (stats.StreakMassimo >= 5) 
            achievements.Add("🔥 STREAK MASTER (5+ consecutive)");
        if (stats.TempoMedio > 0 && stats.TempoMedio < 2.5) 
            achievements.Add("⚡ VELOCISTA (sub 2.5s avg)");
        if (stats.AccuratezzaPercentuale >= 80) 
            achievements.Add("🎯 PRECISION MASTER (80%+ accuracy)");
        if (stats.DistribuzioneTempo.Sotto1s >= 3) 
            achievements.Add("💨 SPEED DEMON (3+ sub-1s answers)");
        if (currentSession.Risposte.Count >= 15) 
            achievements.Add("💪 ENDURANCE (15+ questions)");

        return achievements;
    }

    public bool IsSessionExpired()
    {
        var currentSession = CurrentSession;
        return currentSession != null && 
               currentSession.IsActive && 
               currentSession.TempoRimanente <= TimeSpan.Zero;
    }
}