

namespace NetCorePro.Midleware
{
    public static class JwtTokenAuthExtends
    {
        public static IApplicationBuilder UseJwtTokenAuth(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder is null");
            }
            return builder.UseMiddleware<JwtTokenAuth>();
        }
    }
}
