namespace Contracts.Domain.Projects;

public sealed class Project : SoftDeletableEntity
{
    private Project()
    {
        ProjectNo = string.Empty;
        Title = string.Empty;
        NormalizedProjectNo = string.Empty;
        NormalizedTitle = string.Empty;
    }

    public Project(
        string projectNo,
        string title,
        long projectStatusId,
        DateOnly? startDate,
        DateOnly? endDate,
        long? managerUserId,
        string? description,
        AuditStamp stamp)
        : base(stamp)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("Project end date cannot precede its start date.", nameof(endDate));
        }

        ProjectNo = Guard.RequiredText(projectNo, nameof(projectNo), 100);
        Title = Guard.RequiredText(title, nameof(title), 300);
        ProjectStatusId = Guard.RequiredId(projectStatusId, nameof(projectStatusId));
        StartDate = startDate;
        EndDate = endDate;
        ManagerUserId = managerUserId;
        Description = Guard.OptionalText(description, nameof(description), 2000);
        NormalizedProjectNo = TextNormalizer.SearchKey(ProjectNo);
        NormalizedTitle = TextNormalizer.SearchKey(Title);
    }

    public Project(string projectNo, string title, string? employer, string? department, long projectStatusId,
        DateOnly? startDate, DateOnly? endDate, long? managerUserId, string? description, AuditStamp stamp)
        : this(projectNo, title, projectStatusId, startDate, endDate, managerUserId, description, stamp)
    {
        Employer = Guard.OptionalText(employer, nameof(employer), 250);
        Department = Guard.OptionalText(department, nameof(department), 200);
    }

    public string ProjectNo { get; private set; }
    public string NormalizedProjectNo { get; private set; }

    public string Title { get; private set; }
    public string NormalizedTitle { get; private set; }

    public string? Employer { get; private set; }

    public string? Department { get; private set; }

    public long ProjectStatusId { get; private set; }

    public DateOnly? StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public long? ManagerUserId { get; private set; }

    public string? Description { get; private set; }

    public void Update(string projectNo, string title, string? employer, string? department, long projectStatusId,
        DateOnly? startDate, DateOnly? endDate, long? managerUserId, string? description, AuditStamp stamp)
    {
        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            throw new ArgumentException("Project end date cannot precede its start date.", nameof(endDate));
        ProjectNo = Guard.RequiredText(projectNo, nameof(projectNo), 100);
        NormalizedProjectNo = TextNormalizer.SearchKey(ProjectNo);
        Title = Guard.RequiredText(title, nameof(title), 300);
        NormalizedTitle = TextNormalizer.SearchKey(Title);
        Employer = Guard.OptionalText(employer, nameof(employer), 250);
        Department = Guard.OptionalText(department, nameof(department), 200);
        ProjectStatusId = Guard.RequiredId(projectStatusId, nameof(projectStatusId));
        StartDate = startDate; EndDate = endDate; ManagerUserId = managerUserId;
        Description = Guard.OptionalText(description, nameof(description), 2000);
        Touch(stamp);
    }
}
