var hashLength = 4

func getResourceNameWithHash(name string, hashLength int) string =>
  '${name}-${take(uniqueString(name), hashLength)}'

@export()
func getResourceName(resourceType string, name string) string =>
  '${resourceType}-${name}'

@export()
func getStorageAccountName(name string) string =>
  replace(getResourceNameWithHash('st${name}', hashLength), '-', '')

@export()
func getUniqueResourceName(resourceType string, name string) string =>
  getResourceNameWithHash(getResourceName(resourceType, name), hashLength)

