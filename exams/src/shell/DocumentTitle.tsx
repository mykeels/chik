import { useEffect } from 'react';
import { useMatches } from 'react-router';

const APP_NAME = 'Chik.Exams';

/**
 * Sets document.title from the leaf route's `handle.title` (see main.routes.tsx).
 * Render once inside any route tree that should drive the tab title.
 */
export function DocumentTitle() {
  const matches = useMatches();
  const last = matches[matches.length - 1];
  const pageTitle = (last?.handle as { title?: string } | undefined)?.title;

  useEffect(() => {
    document.title = pageTitle ? `${pageTitle} · ${APP_NAME}` : APP_NAME;
  }, [pageTitle]);

  return null;
}
