public interface IGravitySource
{
    void AddAffectedBody(GravityAffected body);
    void RemoveAffectedBody(GravityAffected body);

    GravitySource GetGravitySource();
}