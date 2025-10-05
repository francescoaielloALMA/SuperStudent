using TabellineVocali.Models;

namespace TabellineVocali.Services;

public class QuizService
{
    private readonly Random _random = new();
    private QuizSession? _currentSession;

    public QuizSession? CurrentSession => _currentSession;
    public bool HasActiveSession => _currentSession?.IsActive == true;

    public QuizSession StartNewSession(int durataMinuti)
    {
        _currentSession = new QuizSession
        {
            DurataMinuti = durataMinuti,
            InizioSessione = DateTime.Now,
            IsActive = true
        };
        return _currentSession;
    }

    public void EndSession()
    {
        if (_currentSession != null)
        {
            _currentSession.IsActive = false;
        }
    }

    public (int a, int b) GenerateQuestion()
    {
        if (_currentSession == null) throw new InvalidOperationException("Nessuna sessione attiva");

        int a, b;
        string chiaveTabellina;
        
        // Evita domande duplicate consecutive
        do
        {
            a = _random.Next(2, 10);
            b = _random.Next(2, 10);
            chiaveTabellina = $"{a}x{b}";
        } while (chiaveTabellina == _currentSession.UltimaDomanda && _currentSession.Risposte.Any());

        _currentSession.UltimaDomanda = chiaveTabellina;
        return (a, b);
    }

    public bool SubmitAnswer(int a, int b, int risposta, double tempoRisposta, int tentativi, bool usedVoice = false)
    {
        if (_currentSession == null) throw new InvalidOperationException("Nessuna sessione attiva");

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

        _currentSession.Risposte.Add(questionResult);

        if (isCorretta)
        {
            // Aggiorna statistiche per risposta corretta
            if (!_currentSession.TempiPerTabellina.ContainsKey(chiaveTabellina))
                _currentSession.TempiPerTabellina[chiaveTabellina] = new List<double>();
            
            _currentSession.TempiPerTabellina[chiaveTabellina].Add(tempoRisposta);
            
            if (tempoRisposta < _currentSession.TempoMinimo) 
                _currentSession.TempoMinimo = tempoRisposta;
            if (tempoRisposta > _currentSession.TempoMassimo) 
                _currentSession.TempoMassimo = tempoRisposta;
            
            _currentSession.StreakCorrente++;
            if (_currentSession.StreakCorrente > _currentSession.StreakMassimo)
                _currentSession.StreakMassimo = _currentSession.StreakCorrente;
        }
        else
        {
            // Aggiorna statistiche per risposta sbagliata
            if (!_currentSession.ErroriPerTabellina.ContainsKey(chiaveTabellina))
                _currentSession.ErroriPerTabellina[chiaveTabellina] = 0;
            
            _currentSession.ErroriPerTabellina[chiaveTabellina]++;
            _currentSession.StreakCorrente = 0;
        }

        return isCorretta;
    }

    public SessionStats CalculateStats()
    {
        if (_currentSession == null) return new SessionStats();

        var totaleDomande = _currentSession.Corrette + _currentSession.Sbagliate;
        var accuratezza = totaleDomande > 0 ? _currentSession.Corrette * 100.0 / totaleDomande : 0;
        
        var risposteValide = _currentSession.Risposte.Where(r => r.TempoRisposta > 0);
        var tempoMedio = risposteValide.Any() ? risposteValide.Average(r => r.TempoRisposta) : 0;
        var tempoMinimo = risposteValide.Any() ? risposteValide.Min(r => r.TempoRisposta) : 0;
        var tempoMassimo = risposteValide.Any() ? risposteValide.Max(r => r.TempoRisposta) : 0;
        
        var stats = new SessionStats
        {
            Corrette = _currentSession.Corrette,
            Sbagliate = _currentSession.Sbagliate,
            TentativiTotali = _currentSession.TentativiTotali,
            TentativiErrati = _currentSession.TentativiErrati,
            AccuratezzaPercentuale = accuratezza,
            TempoMedio = tempoMedio,
            TempoMinimo = tempoMinimo,
            TempoMassimo = tempoMassimo,
            StreakMassimo = _currentSession.StreakMassimo
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
        stats.TabellePiuDifficili = _currentSession.ErroriPerTabellina
            .OrderByDescending(x => x.Value)
            .Take(3)
            .ToList();

        // Tabelline più veloci
        stats.TabelleVeloci = _currentSession.TempiPerTabellina
            .Where(x => x.Value.Any())
            .OrderBy(x => x.Value.Average())
            .Take(3)
            .Select(x => new KeyValuePair<string, double>(x.Key, x.Value.Average()))
            .ToList();

        // Distribuzione tempi
        var risposteCorrette = _currentSession.Risposte.Where(r => r.IsCorretta);
        stats.DistribuzioneTempo = new DistribuzioneTempo
        {
            Sotto1s = risposteCorrette.Count(r => r.TempoRisposta < 1.0),
            Da1a2s = risposteCorrette.Count(r => r.TempoRisposta >= 1.0 && r.TempoRisposta < 2.0),
            Da2a3s = risposteCorrette.Count(r => r.TempoRisposta >= 2.0 && r.TempoRisposta < 3.0),
            Sopra3s = risposteCorrette.Count(r => r.TempoRisposta >= 3.0)
        };

        // Trend accuratezza (prima metà vs seconda metà)
        var metaSessione = _currentSession.Risposte.Count / 2;
        if (metaSessione > 0)
        {
            var accuratezzaPrimaMeta = _currentSession.Risposte.Take(metaSessione).Count(r => r.IsCorretta) * 100.0 / metaSessione;
            var accuratezzaSecondaMeta = _currentSession.Risposte.Skip(metaSessione).Count(r => r.IsCorretta) * 100.0 / (_currentSession.Risposte.Count - metaSessione);
            stats.TrendAccuratezza = accuratezzaSecondaMeta - accuratezzaPrimaMeta;
        }

        return stats;
    }

    private List<string> CalculateAchievements(SessionStats stats)
    {
        var achievements = new List<string>();

        if (_currentSession == null) return achievements;

        if (stats.StreakMassimo >= 5) 
            achievements.Add("🔥 STREAK MASTER (5+ consecutive)");
        if (stats.TempoMedio > 0 && stats.TempoMedio < 2.5) 
            achievements.Add("⚡ VELOCISTA (sub 2.5s avg)");
        if (stats.AccuratezzaPercentuale >= 80) 
            achievements.Add("🎯 PRECISION MASTER (80%+ accuracy)");
        if (stats.DistribuzioneTempo.Sotto1s >= 3) 
            achievements.Add("💨 SPEED DEMON (3+ sub-1s answers)");
        if (_currentSession.Risposte.Count >= 15) 
            achievements.Add("💪 ENDURANCE (15+ questions)");

        return achievements;
    }

    public bool IsSessionExpired()
    {
        return _currentSession != null && 
               _currentSession.IsActive && 
               _currentSession.TempoRimanente <= TimeSpan.Zero;
    }
}