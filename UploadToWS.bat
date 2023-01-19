set SEInstallDir="C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers"
for %%I in (.) do set ParentDirName=%%~nxI
%SEInstallDir%\Bin64\SEWorkshopTool.exe push --mods "%ParentDirName%" --description description.md --message patch_notes.md --webhooks-discord https://discord.com/api/webhooks/886350583026548746/qacuGqaLmHEm9_TSfJq_aLvr0o8kD7DArARa20TlpRzOwWUe9C1zfJilY_8wlJeb9fV4
pause