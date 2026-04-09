import { version } from '../../package.json';

export const fetchLatestVersion = async () => {
  if ('serviceWorker' in navigator) {
    const registrations = await navigator.serviceWorker.getRegistrations();
    for (const registration of registrations) {
      await registration.update().catch((e) => {
        console.warn(e);
      });
      await registration.waiting?.postMessage({ type: 'SKIP_WAITING' });
    }
  }
  await new Promise((resolve) => setTimeout(resolve, 1000));
};

export const isNewVersionAvailable = async () => {
  const versionJson = await fetch('/version.json');
  const versionData = await versionJson.json();
  console.log('versionData', versionData);
  return versionData.version !== version;
};
