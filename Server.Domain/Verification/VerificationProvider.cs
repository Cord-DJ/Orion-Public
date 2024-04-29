namespace Cord.Server.Domain.Verification;

public class VerificationProvider {
    readonly IVerificationRepository verificationRepository;

    public VerificationProvider(IVerificationRepository verificationRepository) {
        this.verificationRepository = verificationRepository;
    }

    public async Task<Verification> PopByCode(string code, VerificationType? verificationType) {
        var verification = await verificationRepository.GetByCode(code);
        if (verification == null) {
            throw new NotFoundException(nameof(Verification), null);
        }

        if (verificationType != null && verification.VerificationType != verificationType) {
            throw new NotFoundException(nameof(Verification), null);
        }

        await verificationRepository.Remove(verification.Id);
        return verification;
    }

    public async Task<string> CreateVerification(ID userId, VerificationType verificationType, int codeLength = 128) {
        var code = StringHelper.GenerateHash(codeLength);

        await verificationRepository.RemoveBy(verificationType, userId);
        await verificationRepository.Add(
            new(
                ID.NewId(),
                userId,
                verificationType,
                code
            )
        );

        return code;
    }
}
