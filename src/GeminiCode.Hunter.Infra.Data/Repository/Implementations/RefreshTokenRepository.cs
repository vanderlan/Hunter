using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository.Implementations;

internal sealed class RefreshTokenRepository(DataContext context)
    : BaseEntityRepository<RefreshToken>(context), IRefreshTokenRepository;
