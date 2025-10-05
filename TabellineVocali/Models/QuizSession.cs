namespace TabellineVocali.Models;

public class QuizSession
{
    public int DurataMinuti { get; set; }
    public DateTime InizioSessione { get; set; }
    public List<QuestionResult> Risposte { get; set; } = new();
    public Dictionary<string, int> ErroriPerTabellina { get; set; } = new();
    public Dictionary<string, List<double>> TempiPerTabellina { get; set; } = new();
    public int StreakCorrente { get; set; }
    public int StreakMassimo { get; set; }
    public double TempoMinimo { get; set; } = double.MaxValue;
    public double TempoMassimo { get; set; }
    public string UltimaDomanda { get; set; } = "";
    public bool IsActive { get; set; }
    
    // Statistiche calcolate
    public int Corrette => Risposte.Count(r => r.IsCorretta);
    public int Sbagliate => Risposte.Count(r => !r.IsCorretta);
    public int TentativiTotali => Risposte.Sum(r => r.TentativiEffettuati);
    public int TentativiErrati => Risposte.Sum(r => r.TentativiEffettuati - 1);
    public double AccuratezzaPercentuale => Risposte.Count > 0 ? (Corrette * 100.0) / Risposte.Count : 0;
    public double TempoMedioRisposta => Risposte.Where(r => r.IsCorretta).Any() ? 
        Risposte.Where(r => r.IsCorretta).Average(r => r.TempoRisposta) : 0;
    public TimeSpan TempoTrascorso => DateTime.Now - InizioSessione;
    public TimeSpan TempoRimanente => TimeSpan.FromMinutes(DurataMinuti) - TempoTrascorso;
}

public class QuestionResult
{
    public int A { get; set; }
    public int B { get; set; }
    public int RispostaCorretta { get; set; }
    public int RispostaUtente { get; set; }
    public bool IsCorretta { get; set; }
    public double TempoRisposta { get; set; }
    public int TentativiEffettuati { get; set; }
    public DateTime Timestamp { get; set; }
    public bool UsedVoice { get; set; }
    
    public string ChiaveTabellina => $"{A}x{B}";
}

public class SessionStats
{
    public int Corrette { get; set; }
    public int Sbagliate { get; set; }
    public int TentativiTotali { get; set; }
    public int TentativiErrati { get; set; }
    public double AccuratezzaPercentuale { get; set; }
    public double TempoMedio { get; set; }
    public double TempoMinimo { get; set; }
    public double TempoMassimo { get; set; }
    public int StreakMassimo { get; set; }
    public double PunteggioComplessivo { get; set; }
    public string Livello { get; set; } = "";
    public List<string> Achievements { get; set; } = new();
    public List<KeyValuePair<string, int>> TabellePiuDifficili { get; set; } = new();
    public List<KeyValuePair<string, double>> TabelleVeloci { get; set; } = new();
    public DistribuzioneTempo DistribuzioneTempo { get; set; } = new();
    public double TrendAccuratezza { get; set; }
}

public class DistribuzioneTempo
{
    public int Sotto1s { get; set; }
    public int Da1a2s { get; set; }
    public int Da2a3s { get; set; }
    public int Sopra3s { get; set; }
}