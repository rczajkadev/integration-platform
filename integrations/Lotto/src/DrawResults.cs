using System;
using System.Collections.Generic;

namespace Integrations.Lotto;

internal sealed class DrawResults
{
    public required DateOnly DrawDateValue { get; init; }

    public required string DrawDate { get; init; }

    public required IEnumerable<int> LottoNumbers { get; init; }

    public required IEnumerable<int> PlusNumbers { get; init; }

    public required string LottoNumbersString { get; init; }

    public string? PlusNumbersString { get; init; }
}
