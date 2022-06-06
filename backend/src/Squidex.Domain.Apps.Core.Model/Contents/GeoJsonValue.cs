﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GeoJSON.Net;
using GeoJSON.Net.Geometry;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.ObjectPool;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Contents
{
    public static class GeoJsonValue
    {
        public static GeoJsonParseResult TryParse(JsonValue value, IJsonSerializer serializer, out GeoJSONObject? geoJSON)
        {
            Guard.NotNull(serializer);
            Guard.NotNull(value);

            geoJSON = null;

            if (value.Type == JsonValueType.Object)
            {
                var obj = value.AsObject;

                if (TryParseGeoJson(obj, serializer, out geoJSON))
                {
                    return GeoJsonParseResult.Success;
                }

                if (!obj.TryGetValue("latitude", out var lat) || lat.Type != JsonValueType.Number || !lat.AsNumber.IsBetween(-90, 90))
                {
                    return GeoJsonParseResult.InvalidLatitude;
                }

                if (!obj.TryGetValue("longitude", out var lon) || lon.Type != JsonValueType.Number || !lon.AsNumber.IsBetween(-180, 180))
                {
                    return GeoJsonParseResult.InvalidLongitude;
                }

                geoJSON = new Point(new Position(lat.AsNumber, lon.AsNumber));

                return GeoJsonParseResult.Success;
            }

            return GeoJsonParseResult.InvalidValue;
        }

        private static bool TryParseGeoJson(ListDictionary<string, JsonValue> obj, IJsonSerializer serializer, out GeoJSONObject? geoJSON)
        {
            geoJSON = null;

            if (!obj.TryGetValue("type", out var type) || type.Type != JsonValueType.String)
            {
                return false;
            }

            try
            {
                using (var stream = DefaultPools.MemoryStream.GetStream())
                {
                    serializer.Serialize(obj, stream, true);

                    stream.Position = 0;

                    geoJSON = serializer.Deserialize<GeoJSONObject>(stream, null, true);

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
