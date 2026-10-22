using Contracts.Application.Security;
using Contracts.Domain.Common;
using Contracts.Domain.Contracts;

namespace Contracts.Application.Operations;

public sealed class OperationsService(IOperationsStore store,IAuthorizationService authorization,
    ICurrentUserService currentUser,ISystemClock clock,Finance.IBusinessDateService businessDate,AlertRuleOptions alertOptions)
{
    public static long PreviewInvoiceTotal(long amountRial,long vatAmountRial)=>InvoiceCalculationPolicy.Total(amountRial,vatAmountRial);
    public static decimal PreviewEvaluationTotal(int quality,int time,int financial,int hse)=>EvaluationScorePolicy.Total(quality,time,financial,hse);
    public async Task<OperationsReferenceData> ReferencesAsync(SecuritySection section,CancellationToken ct){if(section is not(SecuritySection.Amendments or SecuritySection.Guarantees or SecuritySection.Invoices or SecuritySection.Documents or SecuritySection.Alerts or SecuritySection.Evaluation))throw new ArgumentOutOfRangeException(nameof(section));await authorization.RequireAsync(section,SecurityAction.View,ct);return await store.ReferencesAsync(ct);}
    public async Task<IReadOnlyList<AmendmentDto>> AmendmentsAsync(long? contractId,CancellationToken ct){await Require(SecuritySection.Amendments,SecurityAction.View,ct);return await store.AmendmentsAsync(contractId,ct);}
    public async Task<AmendmentDto> CreateAmendmentAsync(AmendmentInput x,CancellationToken ct){await Require(SecuritySection.Amendments,SecurityAction.Add,ct);ValidateAmendment(x);return await store.CreateAmendmentAsync(x,Stamp(),ct);}
    public async Task<AmendmentDto> UpdateAmendmentAsync(AmendmentUpdate x,CancellationToken ct){await Require(SecuritySection.Amendments,SecurityAction.Edit,ct);ValidateAmendment(x.Values);return await store.UpdateAmendmentAsync(x,Stamp(),ct);}
    public async Task DeleteAmendmentAsync(long id,long version,CancellationToken ct){await Require(SecuritySection.Amendments,SecurityAction.Delete,ct);await store.DeleteAmendmentAsync(id,version,Stamp(),ct);}
    public async Task<IReadOnlyList<GuaranteeDto>> GuaranteesAsync(long? c,CancellationToken ct){await Require(SecuritySection.Guarantees,SecurityAction.View,ct);return await store.GuaranteesAsync(c,businessDate.Today,ct);}
    public async Task<GuaranteeDto> CreateGuaranteeAsync(GuaranteeInput x,CancellationToken ct){await Require(SecuritySection.Guarantees,SecurityAction.Add,ct);ValidateGuarantee(x);return await store.CreateGuaranteeAsync(x,Stamp(),businessDate.Today,ct);}
    public async Task<GuaranteeDto> UpdateGuaranteeAsync(GuaranteeUpdate x,CancellationToken ct){await Require(SecuritySection.Guarantees,SecurityAction.Edit,ct);ValidateGuarantee(x.Values);return await store.UpdateGuaranteeAsync(x,Stamp(),businessDate.Today,ct);}
    public async Task DeleteGuaranteeAsync(long id,long v,CancellationToken ct){await Require(SecuritySection.Guarantees,SecurityAction.Delete,ct);await store.DeleteGuaranteeAsync(id,v,Stamp(),ct);}
    public async Task<IReadOnlyList<InvoiceDto>> InvoicesAsync(long? c,CancellationToken ct){await Require(SecuritySection.Invoices,SecurityAction.View,ct);return await store.InvoicesAsync(c,ct);}
    public async Task<InvoiceDto> CreateInvoiceAsync(InvoiceInput x,CancellationToken ct){await Require(SecuritySection.Invoices,SecurityAction.Add,ct);ValidateInvoice(x);return await store.CreateInvoiceAsync(x,Stamp(),ct);}
    public async Task<InvoiceDto> UpdateInvoiceAsync(InvoiceUpdate x,CancellationToken ct){await Require(SecuritySection.Invoices,SecurityAction.Edit,ct);ValidateInvoice(x.Values);return await store.UpdateInvoiceAsync(x,Stamp(),ct);}
    public async Task DeleteInvoiceAsync(long id,long v,CancellationToken ct){await Require(SecuritySection.Invoices,SecurityAction.Delete,ct);await store.DeleteInvoiceAsync(id,v,Stamp(),ct);}
    public async Task<IReadOnlyList<DocumentDto>> DocumentsAsync(long? c,CancellationToken ct){await Require(SecuritySection.Documents,SecurityAction.View,ct);return await store.DocumentsAsync(c,ct);}
    public async Task<DocumentDto> UploadDocumentAsync(DocumentUploadInput x,CancellationToken ct){await Require(SecuritySection.Documents,SecurityAction.Add,ct);return await store.UploadDocumentAsync(x,Stamp(),ct);}
    public async Task<DocumentDto> ReplaceDocumentAsync(DocumentReplaceInput x,CancellationToken ct){await Require(SecuritySection.Documents,SecurityAction.Edit,ct);return await store.ReplaceDocumentAsync(x,Stamp(),ct);}
    public async Task DeleteDocumentAsync(long id,long v,CancellationToken ct){await Require(SecuritySection.Documents,SecurityAction.Delete,ct);await store.DeleteDocumentAsync(id,v,Stamp(),ct);}
    public async Task<IReadOnlyList<AlertDto>> AlertsAsync(long? c,CancellationToken ct){await Require(SecuritySection.Alerts,SecurityAction.View,ct);return await store.AlertsAsync(c,ct);}
    public async Task<AlertGenerationResult> GenerateAlertsAsync(CancellationToken ct){await Require(SecuritySection.Alerts,SecurityAction.Add,ct);return await store.GenerateAlertsAsync(businessDate.Today,alertOptions,Stamp(),ct);}
    public async Task ResolveAlertAsync(long id,long v,CancellationToken ct){await Require(SecuritySection.Alerts,SecurityAction.Edit,ct);await store.ResolveAlertAsync(id,v,Stamp(),ct);}
    public async Task ReopenAlertAsync(long id,long v,CancellationToken ct){await Require(SecuritySection.Alerts,SecurityAction.Edit,ct);await store.ReopenAlertAsync(id,v,Stamp(),ct);}
    public async Task<IReadOnlyList<EvaluationDto>> EvaluationsAsync(long? c,CancellationToken ct){await Require(SecuritySection.Evaluation,SecurityAction.View,ct);return await store.EvaluationsAsync(c,ct);}
    public async Task<EvaluationDto> CreateEvaluationAsync(EvaluationInput x,CancellationToken ct){await Require(SecuritySection.Evaluation,SecurityAction.Add,ct);ValidateEvaluation(x);return await store.CreateEvaluationAsync(x,Stamp(),ct);}
    public async Task<EvaluationDto> UpdateEvaluationAsync(EvaluationUpdate x,CancellationToken ct){await Require(SecuritySection.Evaluation,SecurityAction.Edit,ct);ValidateEvaluation(x.Values);return await store.UpdateEvaluationAsync(x,Stamp(),ct);}
    public async Task DeleteEvaluationAsync(long id,long v,CancellationToken ct){await Require(SecuritySection.Evaluation,SecurityAction.Delete,ct);await store.DeleteEvaluationAsync(id,v,Stamp(),ct);}
    private Task Require(SecuritySection s,SecurityAction a,CancellationToken ct)=>authorization.RequireAsync(s,a,ct);private AuditStamp Stamp()=>new(currentUser.RequireAuthenticated().UserId,clock.UtcNow);
    private static void ValidateAmendment(AmendmentInput x){if(string.IsNullOrWhiteSpace(x.Number))throw new ArgumentException("Amendment number is required.");ArgumentOutOfRangeException.ThrowIfNegative(x.TimeExtensionDays);}
    private static void ValidateGuarantee(GuaranteeInput x){if(string.IsNullOrWhiteSpace(x.Number))throw new ArgumentException("Guarantee number is required.");ArgumentOutOfRangeException.ThrowIfLessThan(x.ExpiryDate,x.IssueDate,nameof(x.ExpiryDate));ArgumentOutOfRangeException.ThrowIfNegativeOrZero(x.AmountRial);}
    private static void ValidateInvoice(InvoiceInput x){if(string.IsNullOrWhiteSpace(x.Number))throw new ArgumentException("Invoice number is required.");ArgumentOutOfRangeException.ThrowIfNegative(x.AmountRial);ArgumentOutOfRangeException.ThrowIfNegative(x.VatAmountRial);if(x.DueDate.HasValue)ArgumentOutOfRangeException.ThrowIfLessThan(x.DueDate.Value,x.Date,nameof(x.DueDate));_=checked(x.AmountRial+x.VatAmountRial);}
    private static void ValidateEvaluation(EvaluationInput x){foreach(int score in new[]{x.QualityScore,x.TimeScore,x.FinancialScore,x.HseScore})if(score is<0 or>100)throw new ArgumentOutOfRangeException(nameof(x));}
}
