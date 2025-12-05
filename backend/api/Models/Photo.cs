namespace api.Models;

public record Photo(
    string Url_165,
    string Url_256,
    string Url_enlarged,
    bool IsMain
);

public record MainPhoto(
    string Url_165,
    string Url_256,
    string Url_enlarged
);