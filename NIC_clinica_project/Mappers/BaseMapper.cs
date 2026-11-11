namespace NIC_clinica_project.Mappers
{
    public abstract class BaseMapper<TKeysDto, TValuesDto, TEntity>
        where TEntity : class, new()
        where TKeysDto : class, new()
        where TValuesDto : class, new()
    {

        private const string IdPropertyForIgnore = "id";
        protected static TTarget CopyMatchingProperties<TSource, TTarget>(TSource source, TTarget target, params string[] ignoredProps)
            where TSource : class
            where TTarget : class
        {
            
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            foreach (var sourceProperty in typeof(TSource).GetProperties())
            {
                if (!sourceProperty.CanRead || ignoredProps.Contains(sourceProperty.Name))
                    continue;

                var targetProperty = typeof(TTarget).GetProperty(sourceProperty.Name);

                if (targetProperty is null || targetProperty.PropertyType != sourceProperty.PropertyType || !targetProperty.CanWrite)
                {
                    continue;
                }

                targetProperty.SetValue(target, sourceProperty.GetValue(source));


            }

            return target;

        }

        public TKeysDto ConvertToDtoWithKeys (TEntity entity)
        {
            return CopyMatchingProperties(entity, new TKeysDto());

        }

        protected abstract void MappingValuebleFields(TEntity entity, TValuesDto dto);
        public TValuesDto ConvertToDtoWithValues(TEntity entity)
        {
            var dto = CopyMatchingProperties(entity, new TValuesDto()); 
            MappingValuebleFields(entity, dto);
            return dto;

        }

        public TEntity ConvertToEfEntity(TKeysDto dto)
        {
            var entity = new TEntity();
            return CopyMatchingProperties(dto, entity, IdPropertyForIgnore);

        }

        public void UpdateEntityFromKeysDto(TKeysDto keysDto, TEntity entity)
        {
            CopyMatchingProperties(keysDto, entity, IdPropertyForIgnore);
        }
    }
}
