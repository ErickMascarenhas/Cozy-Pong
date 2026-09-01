/// <summary>
/// Gerador aleatorio unico do jogo.
///
/// Fora do experimento, delega para UnityEngine.Random e nada muda. Dentro do
/// experimento, usa um System.Random semeado: com a mesma condicao e a mesma
/// semente, dois participantes recebem exatamente a mesma sequencia de pontos
/// de saida, pontos de chegada e lados de retorno.
///
/// Isso importa por dois motivos. Torna a sessao reproduzivel, e portanto
/// reportavel. E remove variancia que, num desenho intra-sujeito, so
/// atrapalharia: se cada participante recebe uma distribuicao espacial
/// diferente, parte da diferenca entre condicoes passa a ser ruido de sorteio.
/// </summary>
public static class ExperimentRandom
{
    private static System.Random _rng;
    private static int _seed;

    public static int Seed { get { return _seed; } }

    public static void Reseed(int seed)
    {
        _seed = seed;
        _rng = new System.Random(seed);
    }

    public static void Clear()
    {
        _rng = null;
        _seed = 0;
    }

    /// <summary>Inteiro em [minInclusive, maxExclusive).</summary>
    public static int Range(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive) return minInclusive;
        if (_rng == null) return UnityEngine.Random.Range(minInclusive, maxExclusive);
        return _rng.Next(minInclusive, maxExclusive);
    }

    /// <summary>Real em [min, max].</summary>
    public static float Range(float min, float max)
    {
        if (_rng == null) return UnityEngine.Random.Range(min, max);
        return min + (float)_rng.NextDouble() * (max - min);
    }

    /// <summary>Real em [0, 1).</summary>
    public static float Value
    {
        get
        {
            if (_rng == null) return UnityEngine.Random.value;
            return (float)_rng.NextDouble();
        }
    }
}
