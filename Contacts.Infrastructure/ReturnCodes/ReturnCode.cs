namespace Contacts.Infrastructure.ReturnCodes;

public record ReturnCode;


public record ReturnCodeSuccess : ReturnCode;

public record ReturnCodeSuccess<T>(T Value) : ReturnCodeSuccess;


public record ReturnCodeFailure : ReturnCode;

public record ReturnCodeNotFound(string? Message) : ReturnCodeFailure;

public record ReturnCodeFailureDetails(string Message, int? Code) : ReturnCodeFailure;

public record ReturnCodeException(Exception Exception) : ReturnCodeFailure;

// for use in tests only
public record ReturnCodeUnexpected: ReturnCode;
