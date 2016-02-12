#
VERSION=`git describe --abbrev=0`
DESCRIBE=`git describe --long`

echo "Setting version to ${VERSION} and the informational version to ${DESCRIBE}"

sed -e 's|\$VER|'${VERSION}'|g' AssemblyInfo.cs.base | sed -e 's|\$DESC|'${DESCRIBE}'|g' > AssemblyInfo.cs
exit 0

sed -e 's|\$VER|'${VERSION}'|g' AssemblyInfo.cs.base > AssemblyInfo.cs.temp
sed -e 's|\$DESC|'${DESCRIBE}'|g' AssemblyInfo.cs.temp > AssemblyInfo.cs
rm AssemblyInfo.cs.temp