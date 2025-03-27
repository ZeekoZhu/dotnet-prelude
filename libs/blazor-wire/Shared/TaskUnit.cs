using R3;

namespace BlazorWire.Shared;

public static class TaskUnit
{
  public static Task<Unit> Result => Task.FromResult(Unit.Default);
}
