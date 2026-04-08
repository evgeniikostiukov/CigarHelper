using System.Security.Claims;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using CigarHelper.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Services;

public class CigarCommentService : ICigarCommentService
{
    private readonly AppDbContext _db;

    public CigarCommentService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<CigarCommentDto>> GetCommentsAsync(
        int? cigarBaseId,
        int? userCigarId,
        ClaimsPrincipal? user,
        CancellationToken cancellationToken = default)
    {
        if (cigarBaseId is > 0)
        {
            var cb = await _db.CigarBases.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == cigarBaseId, cancellationToken);
            if (cb == null || !CanViewCigarBaseComments(cb, user))
                return Array.Empty<CigarCommentDto>();

            return await _db.CigarComments.AsNoTracking()
                .Where(c => c.CigarBaseId == cigarBaseId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CigarCommentDto
                {
                    Id = c.Id,
                    Body = c.Body,
                    CreatedAt = c.CreatedAt,
                    AuthorUserId = c.AuthorUserId,
                    AuthorUsername = c.Author.Username,
                    CigarBaseId = c.CigarBaseId,
                    UserCigarId = c.UserCigarId,
                })
                .ToListAsync(cancellationToken);
        }

        if (userCigarId is > 0)
        {
            var uc = await _db.UserCigars.AsNoTracking()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == userCigarId, cancellationToken);
            if (uc == null || !CanViewUserCigarComments(uc))
                return Array.Empty<CigarCommentDto>();

            return await _db.CigarComments.AsNoTracking()
                .Where(c => c.UserCigarId == userCigarId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CigarCommentDto
                {
                    Id = c.Id,
                    Body = c.Body,
                    CreatedAt = c.CreatedAt,
                    AuthorUserId = c.AuthorUserId,
                    AuthorUsername = c.Author.Username,
                    CigarBaseId = c.CigarBaseId,
                    UserCigarId = c.UserCigarId,
                })
                .ToListAsync(cancellationToken);
        }

        return Array.Empty<CigarCommentDto>();
    }

    public async Task<CigarCommentDto> CreateAsync(
        int authorUserId,
        CreateCigarCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = request.Body.Trim();
        if (string.IsNullOrEmpty(body))
            throw new ArgumentException("Текст комментария не может быть пустым.");

        if (request.CigarBaseId is > 0 && request.UserCigarId is null)
        {
            var cb = await _db.CigarBases.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.CigarBaseId, cancellationToken)
                ?? throw new ArgumentException("Запись справочника не найдена.");

            if (!cb.IsModerated)
                throw new ArgumentException("Комментирование доступно только для промодерированных сигар.");

            var entity = new CigarComment
            {
                AuthorUserId = authorUserId,
                Body = body,
                CigarBaseId = request.CigarBaseId,
                CreatedAt = DateTime.UtcNow,
            };
            _db.CigarComments.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return await MapToDtoAsync(entity.Id, cancellationToken);
        }

        if (request.UserCigarId is > 0 && request.CigarBaseId is null)
        {
            var uc = await _db.UserCigars
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == request.UserCigarId, cancellationToken)
                ?? throw new ArgumentException("Сигара в коллекции не найдена.");

            if (!uc.User.IsProfilePublic)
                throw new ArgumentException("Комментирование доступно только для публичных коллекций.");

            if (uc.UserId == authorUserId)
                throw new ArgumentException("Нельзя оставлять комментарии к собственной сигаре.");

            var entity = new CigarComment
            {
                AuthorUserId = authorUserId,
                Body = body,
                UserCigarId = request.UserCigarId,
                CreatedAt = DateTime.UtcNow,
            };
            _db.CigarComments.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return await MapToDtoAsync(entity.Id, cancellationToken);
        }

        throw new ArgumentException("Укажите ровно одну цель: cigarBaseId или userCigarId.");
    }

    public async Task<bool> TryDeleteAsync(
        int commentId,
        int requesterUserId,
        bool requesterIsAdmin,
        CancellationToken cancellationToken = default)
    {
        var c = await _db.CigarComments.FirstOrDefaultAsync(x => x.Id == commentId, cancellationToken);
        if (c == null)
            return false;

        if (!requesterIsAdmin && c.AuthorUserId != requesterUserId)
            return false;

        _db.CigarComments.Remove(c);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static bool CanViewCigarBaseComments(CigarBase cb, ClaimsPrincipal? user)
    {
        if (cb.IsModerated)
            return true;
        return user != null && (user.IsInRole(nameof(Role.Moderator)) || user.IsInRole(nameof(Role.Admin)));
    }

    private static bool CanViewUserCigarComments(UserCigar uc) => uc.User.IsProfilePublic;

    private async Task<CigarCommentDto> MapToDtoAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.CigarComments.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CigarCommentDto
            {
                Id = c.Id,
                Body = c.Body,
                CreatedAt = c.CreatedAt,
                AuthorUserId = c.AuthorUserId,
                AuthorUsername = c.Author.Username,
                CigarBaseId = c.CigarBaseId,
                UserCigarId = c.UserCigarId,
            })
            .FirstAsync(cancellationToken);
    }
}
