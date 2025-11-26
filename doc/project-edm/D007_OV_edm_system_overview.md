# Electronic Document Management (EDM) System - Overview

## Executive Summary

Transformation of the dotnet-angular-catalog project into a comprehensive Electronic Document Management (EDM) system. The system will enable organizations to store, manage, track, and retrieve electronic documents with version control, access management, and workflow automation.

## System Purpose

The EDM system provides a centralized platform for:

- **Document Storage & Organization**: Secure storage with hierarchical folder structures
- **Version Control**: Track document changes and maintain revision history
- **Access Control**: Role-based permissions and document-level security
- **Workflow Management**: Document approval and review processes
- **Search & Retrieval**: Full-text search and metadata-based filtering
- **Audit Trail**: Complete tracking of document activities
- **Collaboration**: Document sharing, comments, and notifications

## Core Features

### 1. Document Management

- Upload documents (PDF, DOCX, XLSX, images, etc.)
- Folder/category hierarchy
- Document metadata (title, description, tags, custom fields)
- Version control with check-in/check-out
- Document preview
- Bulk operations (upload, download, move, delete)

### 2. User & Access Management

- User authentication and authorization
- Role-based access control (Admin, Manager, User, Viewer)
- Document-level permissions (read, write, delete, share)
- User groups and departments
- Activity logging per user

### 3. Workflow & Approval

- Configurable approval workflows
- Document review processes
- Task assignments
- Email notifications
- Status tracking (Draft, In Review, Approved, Rejected, Published)

### 4. Search & Discovery

- Full-text search across document content
- Metadata filtering (date, author, tags, type)
- Advanced search with boolean operators
- Recent documents and favorites
- Related document suggestions

### 5. Audit & Compliance

- Complete audit trail (who, what, when)
- Document retention policies
- Compliance reporting
- Export logs for auditing

### 6. Integration & Collaboration

- Email integration
- Comments and annotations
- Document sharing (internal/external)
- Real-time notifications
- Export/import capabilities

## Technology Stack

### Backend (.NET 8)

- **ASP.NET Core Web API**: RESTful services
- **Entity Framework Core**: ORM and database access
- **SQL Server**: Primary data storage
- **Azure Blob Storage / File System**: Document storage
- **SignalR**: Real-time notifications
- **JWT**: Authentication tokens
- **Hangfire**: Background jobs (retention policies, cleanup)

### Frontend (Angular 17)

- **Angular Material**: UI components
- **RxJS**: Reactive programming
- **NgRx**: State management
- **Angular CDK**: Drag & drop, virtual scrolling
- **PDF.js**: Document preview
- **Chart.js**: Analytics and dashboards

### Infrastructure

- **Docker**: Containerization
- **SQL Server 2022**: Database with Full-Text Search
- **File System**: Document storage (local or network share)
  - Alternative: Azure Blob Storage for cloud deployment

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Angular Frontend                        │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │Documents │  │ Search   │  │Workflows │  │  Admin   │     │
│  │ Manager  │  │ & Filter │  │& Approval│  │  Panel   │     │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │
└────────────────────┬────────────────────────────────────────┘
                     │ HTTP/WebSocket
┌────────────────────▼────────────────────────────────────────┐
│                   .NET 8 Web API                            │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │Documents │  │  Users   │  │Workflows │  │  Search  │     │
│  │Controller│  │Controller│  │Controller│  │Controller│     │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              Business Logic Layer                    │   │
│  │  Services | Validators | Authorization | Workflows   │   │
│  └──────────────────────────────────────────────────────┘   │
└────────────────────┬────────────────────────────────────────┘
                     │
        ┌────────────┴────────────┐
        │                         │
┌───────▼────────┐      ┌─────────▼──────────┐
│   SQL Server   │      │  File Storage      │
│                │      │ (Blob/FileSystem)  │
│ - Metadata     │      │ - Documents        │
│ - Users        │      │ - Versions         │
│ - Permissions  │      │ - Thumbnails       │
│ - Audit Logs   │      │                    │
└────────────────┘      └────────────────────┘
```

## Core Domain Entities

### Document

- ID, Title, Description
- File Path, File Size, MIME Type
- Version Number, Is Latest Version
- Created/Modified By/At
- Status (Draft, Published, Archived)
- Parent Folder ID
- Tags, Custom Metadata

### Folder

- ID, Name, Description
- Parent Folder ID (hierarchical)
- Permissions
- Created/Modified By/At

### User

- ID, Username, Email
- Password Hash
- Full Name, Department
- Role (Admin, Manager, User, Viewer)
- Is Active
- Created/Modified At

### Permission

- ID, User/Group ID
- Document/Folder ID
- Permission Type (Read, Write, Delete, Share)
- Granted By/At

### Version

- ID, Document ID
- Version Number, File Path
- Comment, Changed By/At
- File Size, Checksum

### Workflow

- ID, Name, Description
- Steps (Review → Approval → Publish)
- Current Step, Status
- Assigned To, Due Date

### AuditLog

- ID, User ID
- Action Type (View, Edit, Delete, Share, Download)
- Document ID, Details
- IP Address, Timestamp

## User Roles & Permissions

### Administrator

- Full system access
- User management
- System configuration
- Audit log access
- Retention policy management

### Manager

- Document approval
- Workflow configuration
- Team document management
- Reports and analytics

### User

- Upload/edit own documents
- Share documents
- Participate in workflows
- Search and view permitted documents

### Viewer

- Read-only access
- View permitted documents
- Download documents
- Search

## Development Phases

### Phase 1: Foundation (Weeks 1-2)

- Database schema redesign
- Core entities and migrations
- Authentication & authorization
- Basic CRUD for documents and folders
- File upload/download functionality

### Phase 2: Core Features (Weeks 3-4)

- Version control system
- Permission management
- Folder hierarchy
- Search functionality
- Audit logging

### Phase 3: Advanced Features (Weeks 5-6)

- Workflow engine
- Document preview
- Real-time notifications
- Advanced search
- Analytics dashboard

### Phase 4: Polish & Enhancement (Weeks 7-8)

- UI/UX improvements
- Performance optimization
- Testing and bug fixes
- Documentation
- Deployment automation

## Success Metrics

- **Performance**: Upload/download < 3s for files up to 10MB
- **Security**: Role-based access with 100% audit coverage
- **Usability**: < 3 clicks to access any document
- **Reliability**: 99.9% uptime
- **Scalability**: Support 10,000+ documents, 500+ concurrent users

## Next Steps

1. **Review & Approve**: Stakeholder review of this overview
2. **Detailed Design**: Create detailed domain models and API specifications
3. **Database Design**: Comprehensive schema with relationships
4. **API Design**: RESTful endpoint documentation
5. **UI/UX Mockups**: Wireframes and user flows
6. **Implementation Plan**: Detailed sprint planning

## Storage Architecture Decision

### SQL Server Only (Recommended Approach)

**Why SQL Server is sufficient:**

- **Structured Metadata**: Document properties, users, permissions are inherently relational
- **ACID Compliance**: Critical for audit trails and version control integrity
- **Full-Text Search**: Built-in capability eliminates need for Elasticsearch
- **Proven Scalability**: Handles 10,000+ documents and 500+ concurrent users easily
- **Simpler Architecture**: One database technology reduces complexity and maintenance
- **File Management**: File system for actual documents, SQL Server for metadata

**Why NOT MongoDB:**

- Document metadata is structured (not schema-less)
- Strong relational requirements (permissions, folders, workflows)
- ACID transactions are non-negotiable for EDM systems
- No need for horizontal scaling at this scope
- SQL Server Full-Text Search performs well for 10K-100K documents

**Storage Strategy:**

```
SQL Server (Metadata + Search)
├── Documents table (title, tags, status, filePath)
├── Versions table (version history)
├── Full-Text Catalog (indexed content)
└── Audit logs

File System (Physical Files)
└── /documents/{year}/{month}/{documentId}_{version}.ext
```

**Future Scalability Options:**

- 100K+ documents: Consider Azure Blob Storage
- Multi-region: Add read replicas
- Heavy search: Add Elasticsearch only if SQL Full-Text becomes a bottleneck

## Questions to Address

1. **Notification Method**: Email, in-app, or both?
2. **Mobile Support**: Responsive web or native mobile apps?
3. **Integration Requirements**: Third-party systems (email, SharePoint, etc.)?
4. **Compliance Needs**: GDPR, HIPAA, or other regulatory requirements?
5. **Deployment Target**: On-premise, cloud (Azure/AWS), or hybrid?
6. **File Storage Location**: Local file system, network share, or cloud blob storage?

---

**Document Version**: 1.0  
**Last Updated**: November 25, 2025  
**Author**: EDM Project Team  
**Status**: Draft - Pending Review
