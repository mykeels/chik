# Claude Instructions

This is a react web application for the Chik.Exams project.

- Use React Query for data fetching and caching.
  - See and leverage the hook pattern used in [src/services/chikexams.hooks.ts](src/services/chikexams.hooks.ts) for fetching data and caching.
- Use Tailwind CSS for styling.
- Use Lucide Icons for icons.
- Use React Router for routing.
- Use React Hook Form for form handling.
- Use React Markdown for rendering markdown.
- Use MUI for components.
- See [UIUX.md](UIUX.md) for the UI/UX design.
- See [PRD.md](../Chik.Exams/PRD.md) for the Product Requirements Document.
- Each component file should have a corresponding *.stories.tsx file for documentation and testing.
  - Use @storybook/test's 'play' function to test the component.
- Where necessary, use the playwright MCP server to view stories at http://localhost:6006