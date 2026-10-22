namespace Contracts.Domain.ReferenceData;

public sealed class ContractType : ReferenceEntity { private ContractType() { } public ContractType(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class ContractStatus : ReferenceEntity { private ContractStatus() { } public ContractStatus(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class PartyType : ReferenceEntity { private PartyType() { } public PartyType(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class ProjectStatus : ReferenceEntity { private ProjectStatus() { } public ProjectStatus(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class Bank : ReferenceEntity { private Bank() { } public Bank(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class PaymentType : ReferenceEntity { private PaymentType() { } public PaymentType(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class GuaranteeType : ReferenceEntity { private GuaranteeType() { } public GuaranteeType(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class GuaranteeStatus : ReferenceEntity { private GuaranteeStatus() { } public GuaranteeStatus(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class InvoiceType : ReferenceEntity { private InvoiceType() { } public InvoiceType(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class PaymentStatus : ReferenceEntity { private PaymentStatus() { } public PaymentStatus(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class DocumentType : ReferenceEntity { private DocumentType() { } public DocumentType(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class AlertType : ReferenceEntity { private AlertType() { } public AlertType(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class AlertStatus : ReferenceEntity { private AlertStatus() { } public AlertStatus(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class Priority : ReferenceEntity { private Priority() { } public Priority(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class TransactionType : ReferenceEntity { private TransactionType() { } public TransactionType(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }
public sealed class FinancialTransactionStatus : ReferenceEntity { private FinancialTransactionStatus() { } public FinancialTransactionStatus(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp) : base(id, code, titleFa, sortOrder, isActive, stamp) { } }

public sealed class Currency : ReferenceEntity
{
    private Currency() { }

    public Currency(long id, string code, string titleFa, int sortOrder, bool isActive, int scale, AuditStamp stamp)
        : base(id, code, titleFa, sortOrder, isActive, stamp)
    {
        if (scale is < 0 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(scale));
        }

        Scale = scale;
    }

    public int Scale { get; private set; }
}
